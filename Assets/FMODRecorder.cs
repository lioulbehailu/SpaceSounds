using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODRecorder : MonoBehaviour
{
    public static FMODRecorder Instance;

    [Header("FMOD References")]
    [Tooltip("Drag one of your main planet emitters here to detect loop length.")]
    public StudioEventEmitter masterMusicEmitter;

    // --- Capture state (accessed from the native audio thread, so keep it lock-protected) ---
    private static readonly object sampleLock = new object();
    private static List<float> capturedSamples = new List<float>();
    private static volatile bool isRecording = false;

    private int sampleRate = 44100;
    private static int capturedChannels = 2;

    // --- FMOD DSP objects ---
    private FMOD.ChannelGroup masterChannelGroup;
    private FMOD.DSP captureDsp;
    private FMOD.DSP_DESCRIPTION dspDescription;
    private static FMOD.DSP_READ_CALLBACK readCallback;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        SetupCaptureDSP();
    }

    private void SetupCaptureDSP()
    {
        FMOD.System coreSystem = RuntimeManager.CoreSystem;

        coreSystem.getSoftwareFormat(out sampleRate, out FMOD.SPEAKERMODE speakerMode, out int numRawSpeakers);
        Debug.Log($"[FMODRecorder] FMOD software format sample rate: {sampleRate}");

        FMOD.RESULT res = coreSystem.getMasterChannelGroup(out masterChannelGroup);
        if (res != FMOD.RESULT.OK)
        {
            Debug.LogError($"[FMODRecorder] Could not get FMOD master channel group: {res}");
            return;
        }

        // Static delegate so it survives GC / domain reloads and works with IL2CPP AOT.
        readCallback = CaptureReadCallback;

        dspDescription = new FMOD.DSP_DESCRIPTION();
        dspDescription.numinputbuffers = 1;
        dspDescription.numoutputbuffers = 1;
        dspDescription.read = readCallback;

        res = coreSystem.createDSP(ref dspDescription, out captureDsp);
        if (res != FMOD.RESULT.OK)
        {
            Debug.LogError($"[FMODRecorder] Failed to create capture DSP: {res}");
            return;
        }

        // TAIL = after every other DSP on the master bus, i.e. the final mixed signal.
        res = masterChannelGroup.addDSP(FMOD.CHANNELCONTROL_DSP_INDEX.TAIL, captureDsp);
        if (res != FMOD.RESULT.OK)
        {
            Debug.LogError($"[FMODRecorder] Failed to attach capture DSP to master bus: {res}");
            return;
        }

        Debug.Log("[FMODRecorder] Capture DSP attached to FMOD master channel group.");
    }

    // Must be static + [MonoPInvokeCallback] so IL2CPP can call it from native code.
    [AOT.MonoPInvokeCallback(typeof(FMOD.DSP_READ_CALLBACK))]
    private static FMOD.RESULT CaptureReadCallback(ref FMOD.DSP_STATE dsp_state, IntPtr inbuffer, IntPtr outbuffer, uint length, int inchannels, ref int outchannels)
    {
        int sampleCount = (int)length * inchannels;

        // Always pass audio straight through so playback is unaffected.
        // (inbuffer/outbuffer are the same size here since numinputbuffers == numoutputbuffers == 1
        // and we didn't force a channel count, so inchannels == outchannels.)
        byte[] raw = null;
        if (sampleCount > 0)
        {
            raw = new byte[sampleCount * sizeof(float)];
            Marshal.Copy(inbuffer, raw, 0, raw.Length);
            Marshal.Copy(raw, 0, outbuffer, raw.Length);
        }

        if (isRecording && raw != null)
        {
            float[] samples = new float[sampleCount];
            Buffer.BlockCopy(raw, 0, samples, 0, raw.Length);

            capturedChannels = inchannels;

            lock (sampleLock)
            {
                capturedSamples.AddRange(samples);
            }
        }

        return FMOD.RESULT.OK;
    }

    private void OnDestroy()
    {
        if (masterChannelGroup.hasHandle() && captureDsp.hasHandle())
        {
            masterChannelGroup.removeDSP(captureDsp);
        }
        if (captureDsp.hasHandle())
        {
            captureDsp.release();
        }
    }

    public IEnumerator InstantExportOneLoopRoutine()
    {
        if (masterMusicEmitter == null)
        {
            Debug.LogError("[FMODRecorder] Master Emitter missing in Inspector!");
            yield break;
        }

        EventDescription eventDesc = masterMusicEmitter.EventDescription;
        if (!eventDesc.isValid())
        {
            Debug.LogError("[FMODRecorder] Invalid FMOD Event Description.");
            yield break;
        }

        EventInstance eventInstance = masterMusicEmitter.EventInstance;
        if (!eventInstance.isValid())
        {
            Debug.LogError("[FMODRecorder] Invalid FMOD Event Instance - is the emitter playing?");
            yield break;
        }

        eventDesc.getLength(out int lengthInMs);
        float loopDurationSeconds = lengthInMs / 1000f;
        if (loopDurationSeconds <= 0) loopDurationSeconds = 26.7f;

        Debug.Log("[FMODRecorder] Waiting for loop to restart before capturing...");

        // Poll the timeline position and wait for it to wrap back near 0.
        // That's when a fresh loop cycle begins, so we start recording exactly
        // at the start of the loop instead of at a random offset into it.
        eventInstance.getTimelinePosition(out int lastPosition);
        float safetyTimer = 0f;
        while (true)
        {
            eventInstance.getTimelinePosition(out int currentPosition);
            if (currentPosition < lastPosition - 1000)
            {
                break; // position dropped sharply = loop just restarted
            }
            lastPosition = currentPosition;

            safetyTimer += Time.deltaTime;
            if (safetyTimer > loopDurationSeconds + 5f)
            {
                Debug.LogWarning("[FMODRecorder] Timed out waiting for loop restart, capturing anyway.");
                break;
            }

            yield return null;
        }

        Debug.Log($"[FMODRecorder] Loop restarted, starting live recording for {loopDurationSeconds:F2} seconds...");

        lock (sampleLock)
        {
            capturedSamples.Clear();
        }

        isRecording = true;

        yield return new WaitForSeconds(loopDurationSeconds);

        isRecording = false;

        int count;
        lock (sampleLock)
        {
            count = capturedSamples.Count;
        }

        Debug.Log($"[FMODRecorder] Recording complete! Captured {count} samples. Saving WAV...");

        SaveSamplesToWavFile();
    }

    private void SaveSamplesToWavFile()
    {
        string targetPath = Path.Combine(Application.persistentDataPath, "latest_track.wav");

        float[] sampleArray;
        lock (sampleLock)
        {
            sampleArray = capturedSamples.ToArray();
        }

        if (sampleArray.Length == 0)
        {
            Debug.LogError("[SAVE FAILED] No audio samples captured. Check that the capture DSP attached successfully in Start() (see Console for '[FMODRecorder] Capture DSP attached...') and that music is actually playing during the recording window.");
            return;
        }

        short[] pcmShorts = new short[sampleArray.Length];
        for (int i = 0; i < sampleArray.Length; i++)
        {
            float clamped = Mathf.Clamp(sampleArray[i], -1.0f, 1.0f);
            pcmShorts[i] = (short)(clamped * 32767f);
        }

        byte[] byteData = new byte[pcmShorts.Length * 2];
        Buffer.BlockCopy(pcmShorts, 0, byteData, 0, byteData.Length);

        int channels = capturedChannels;

        try
        {
            using (FileStream fs = new FileStream(targetPath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + byteData.Length);
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(sampleRate * channels * 2);
                writer.Write((short)(channels * 2));
                writer.Write((short)16);
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(byteData.Length);
                writer.Write(byteData);
            }

            Debug.Log($"[SAVE SUCCESS] Valid WAV file created! File size: {new FileInfo(targetPath).Length / 1024} KB at: {targetPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SAVE FAILED] Error writing WAV file: {e.Message}");
        }
    }
}
