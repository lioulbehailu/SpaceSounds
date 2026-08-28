using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SoundtrackUploader : MonoBehaviour
{
    public static SoundtrackUploader Instance;

    [Header("Server Configuration")]
    [Tooltip("Your web backend endpoint URL. Leave blank when testing locally.")]
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

        if (string.IsNullOrEmpty(serverUploadUrl))
        {
            Debug.Log($"[Test Mode] Audio exported locally to: {filePath}");
            yield break;
        }

        byte[] fileData = File.ReadAllBytes(filePath);

        WWWForm form = new WWWForm();
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
