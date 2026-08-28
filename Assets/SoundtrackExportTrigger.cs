using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

public class SoundtrackExportTrigger : MonoBehaviour
{
    [Header("UI Feedback")]
    public TextMeshProUGUI statusText;
    public Image backgroundImage;

    private float holdTimer = 0f;
    private const float REQUIRED_HOLD_TIME = 3.0f;

    public static bool IsExporting { get; private set; } = false;

    void Start()
    {
        SetUIVisibility(false);
    }

    void Update()
    {
        if (IsExporting) return;

        bool inputActive = CheckExportInputs();

        if (inputActive)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= REQUIRED_HOLD_TIME)
            {
                holdTimer = 0f;
                StartCoroutine(ExportSequence());
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private bool CheckExportInputs()
    {
#if UNITY_EDITOR
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit0Key.isPressed || Keyboard.current.numpad0Key.isPressed)
            {
                return true;
            }
        }
#endif

        bool leftFaceButton = OVRInput.Get(OVRInput.Button.Three) || OVRInput.Get(OVRInput.Button.Four);
        bool leftTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) > 0.7f ||
                           OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.7f;

        bool rightFaceButton = OVRInput.Get(OVRInput.Button.One) || OVRInput.Get(OVRInput.Button.Two);
        bool rightTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.7f ||
                            OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.7f;

        return (leftFaceButton && leftTrigger) || (rightFaceButton && rightTrigger);
    }

    private IEnumerator ExportSequence()
    {
        IsExporting = true;
        UpdateUI("Recording soundtrack...", true);

        if (FMODRecorder.Instance != null)
        {
            yield return StartCoroutine(FMODRecorder.Instance.InstantExportOneLoopRoutine());
        }

        if (SoundtrackUploader.Instance != null)
        {
            UpdateUI("Uploading track...", true);
            yield return StartCoroutine(SoundtrackUploader.Instance.UploadLatestTrackRoutine());
        }

        UpdateUI("Export Successful!", true);
        yield return new WaitForSeconds(4f);

        SetUIVisibility(false);
        IsExporting = false;
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
