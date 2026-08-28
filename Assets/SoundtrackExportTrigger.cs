using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Added for Image reference
using TMPro;

// Attach to SoundtrackExportManager GameObject
public class SoundtrackExportTrigger : MonoBehaviour
{
    [Header("UI Feedback")]
    [Tooltip("Assign your TextMeshPro component for in-VR export messages")]
    public TextMeshProUGUI statusText;

    [Tooltip("Assign the background Image component here")]
    public Image backgroundImage;

    private float holdTimer = 0f;
    private const float REQUIRED_HOLD_TIME = 3.0f;
    private bool isExporting = false;

    void Start()
    {
        // Keep UI text and background invisible by default when idle
        SetUIVisibility(false);
    }

    void Update()
    {
        if (isExporting) return;

        // Check Left Controller: X button (Button.Three) + Primary Index Trigger (>80%)
        bool leftPressed = OVRInput.Get(OVRInput.Button.Three) &&
                            OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) > 0.8f;

        // Check Right Controller: A button (Button.One) + Primary Index Trigger (>80%)
        bool rightPressed = OVRInput.Get(OVRInput.Button.One) &&
                             OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.8f;

        if (leftPressed || rightPressed)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= REQUIRED_HOLD_TIME)
            {
                StartCoroutine(ExportSequence());
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private IEnumerator ExportSequence()
    {
        isExporting = true;

        // Show UI with progress message
        UpdateUI("Exporting sound...", true);

        // Step 1: Render 1 loop offline from 00:00 to end using current mix state
        yield return StartCoroutine(FMODRecorder.Instance.InstantExportOneLoopRoutine());

        // Step 2: Upload MP3 to server endpoint
        yield return StartCoroutine(SoundtrackUploader.Instance.UploadLatestTrackRoutine());

        // Step 3: Display success message for 5 seconds
        UpdateUI("Export Successful!", true);

        yield return new WaitForSeconds(5f);

        // Reset UI text and hide background image
        SetUIVisibility(false);
        isExporting = false;
    }

    private void UpdateUI(string message, bool visible)
    {
        if (statusText != null) statusText.text = message;
        SetUIVisibility(visible);
    }

    private void SetUIVisibility(bool visible)
    {
        if (statusText != null) statusText.enabled = visible;
        if (backgroundImage != null) backgroundImage.enabled = visible;
    }
}
