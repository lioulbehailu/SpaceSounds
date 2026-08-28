using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

// Attach to SoundtrackExportManager GameObject
public class SoundtrackUploader : MonoBehaviour
{
    public static SoundtrackUploader Instance;

    [Header("Server Configuration")]
    [Tooltip("Your web backend endpoint URL. Leave blank when testing locally on Quest.")]
    public string serverUploadUrl = "";

    void Awake()
    {
        Instance = this;
    }

    public IEnumerator UploadLatestTrackRoutine()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "latest_track.wav");

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Uploader: No local WAV file found at target path.");
            yield break;
        }

        // Test mode: gracefully skips HTTP POST if no URL is provided yet
        if (string.IsNullOrEmpty(serverUploadUrl))
        {
            Debug.Log($"[Test Mode] Audio exported locally to: {filePath}. (Set serverUploadUrl when backend is ready).");
            yield break;
        }

        byte[] fileData = File.ReadAllBytes(filePath);

        WWWForm form = new WWWForm();
        // Updated to target .wav filename and audio/wav MIME type
        form.AddBinaryData("file", fileData, "latest_track.wav", "audio/wav");

        using (UnityWebRequest www = UnityWebRequest.Post(serverUploadUrl, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Upload Success! Server file overwritten.");
            }
            else
            {
                Debug.LogError($"Upload Error: {www.error} | Status Code: {www.responseCode}");
            }
        }
    }
}
