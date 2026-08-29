using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SoundtrackUploader : MonoBehaviour
{
    public static SoundtrackUploader Instance;

    [Header("Firebase Configuration")]
    [Tooltip("From Firebase Console > Project Settings > General > Your apps > Web API Key")]
    public string firebaseApiKey = "AIzaSyCZFajPZe-9y0mfpBbOdt3l_WGXtjqRxNE";

    [Tooltip("Your Storage bucket name, e.g. spacesound-5ed0d.firebasestorage.app")]
    public string firebaseBucket = "spacesound-5ed0d.firebasestorage.app";

    [Tooltip("Fixed path/filename in Storage. Keep this the same so every upload overwrites the last one.")]
    public string storagePath = "tracks/latest_track.wav";

    [Tooltip("Filename the browser will save the download as.")]
    public string downloadFileName = "SpaceSoundsTrack.wav";

    [Serializable]
    private class AuthResponse
    {
        public string idToken;
    }

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

        if (string.IsNullOrEmpty(firebaseApiKey) || string.IsNullOrEmpty(firebaseBucket))
        {
            Debug.Log($"[Test Mode] Firebase not configured. Audio exported locally to: {filePath}");
            yield break;
        }

        // Step A: sign in anonymously to get a short-lived auth token.
        string idToken = null;
        yield return StartCoroutine(GetAnonymousToken(token => idToken = token));

        if (string.IsNullOrEmpty(idToken))
        {
            Debug.LogError("Uploader: Failed to obtain Firebase auth token, aborting upload.");
            yield break;
        }

        // Step B: upload (overwrite) the file at the fixed storage path.
        byte[] fileData = File.ReadAllBytes(filePath);
        string encodedPath = UnityWebRequest.EscapeURL(storagePath); // encodes "/" as %2F
        string uploadUrl = $"https://firebasestorage.googleapis.com/v0/b/{firebaseBucket}/o/{encodedPath}?uploadType=media";

        bool uploadSucceeded = false;

        using (UnityWebRequest www = new UnityWebRequest(uploadUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(fileData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.method = UnityWebRequest.kHttpVerbPOST;
            www.SetRequestHeader("Content-Type", "audio/wav");
            www.SetRequestHeader("Authorization", "Bearer " + idToken);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Upload Success! Firebase file overwritten.");
                Debug.Log($"[Download URL] https://firebasestorage.googleapis.com/v0/b/{firebaseBucket}/o/{encodedPath}?alt=media");
                uploadSucceeded = true;
            }
            else
            {
                Debug.LogError($"Upload Error: {www.error} | Status Code: {www.responseCode} | Body: {www.downloadHandler.text}");
            }
        }

        // Step C: force the file to download instead of opening in-browser.
        if (uploadSucceeded)
        {
            yield return StartCoroutine(SetContentDisposition(encodedPath, idToken));
        }
    }

    private IEnumerator SetContentDisposition(string encodedPath, string idToken)
    {
        string metadataUrl = $"https://firebasestorage.googleapis.com/v0/b/{firebaseBucket}/o/{encodedPath}";
        string disposition = $"attachment; filename=\"{downloadFileName}\"";

        // Escape the inner double-quotes so the JSON body stays valid.
        string escapedDisposition = disposition.Replace("\"", "\\\"");
        string jsonBody = "{\"contentDisposition\":\"" + escapedDisposition + "\"}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest www = new UnityWebRequest(metadataUrl, "PATCH"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + idToken);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Content-Disposition set: file will now force-download.");
            }
            else
            {
                Debug.LogError($"Metadata Update Error: {www.error} | Body: {www.downloadHandler.text}");
            }
        }
    }

    private IEnumerator GetAnonymousToken(Action<string> onComplete)
    {
        string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={firebaseApiKey}";
        string jsonBody = "{\"returnSecureToken\":true}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AuthResponse response = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
                onComplete?.Invoke(response.idToken);
            }
            else
            {
                Debug.LogError($"Auth Error: {www.error} | Body: {www.downloadHandler.text}");
                onComplete?.Invoke(null);
            }
        }
    }
}
