using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Passive display component — attach to any XR interactable (satellite or planet).
// All input handling is done by ControllerCheatSheetHandler on the controllers.
public class SatelliteCheatSheetTrigger : MonoBehaviour
{
    [Header("Cheat Sheet")]
    [SerializeField] private Sprite cheatSheetSprite;

    public static SatelliteCheatSheetTrigger CurrentlyOpen { get; private set; }
    public bool IsOpen { get; private set; }

    public void Toggle(IXRInteractor fromInteractor)
    {
        if (IsOpen) Close();
        else Open(fromInteractor);
    }

    public void Open(IXRInteractor fromInteractor)
    {
        if (CurrentlyOpen != null && CurrentlyOpen != this)
            CurrentlyOpen.Close();

        if (UiManager.Instance == null) return;
        var canvasObj = UiManager.Instance.GetCanvasObject();
        var followScript = UiManager.Instance.GetFollowScript();
        if (canvasObj == null || followScript == null) return;

        followScript.UpdateSpriteContent(cheatSheetSprite);
        SetFollowTarget(fromInteractor, followScript);
        canvasObj.transform.position = followScript.controllerTarget.TransformPoint(followScript.localOffset);
        canvasObj.SetActive(true);

        IsOpen = true;
        CurrentlyOpen = this;
    }

    public void Close()
    {
        if (!IsOpen) return;

        if (UiManager.Instance != null)
        {
            UiManager.Instance.GetCanvasObject()?.SetActive(false);

            var followScript = UiManager.Instance.GetFollowScript();
            if (followScript != null) followScript.controllerTarget = null;
        }

        IsOpen = false;
        if (CurrentlyOpen == this) CurrentlyOpen = null;
    }

    void OnDisable() => Close();

    private void SetFollowTarget(IXRInteractor interactor, SmoothFollowUI followScript)
    {
        bool isLeftHand = interactor is XRBaseInteractor xrInteractor
            ? xrInteractor.handedness == InteractorHandedness.Left
            : interactor.transform.name.IndexOf("left", StringComparison.OrdinalIgnoreCase) >= 0;

        Vector3 offset = followScript.localOffset;
        offset.x = isLeftHand ? Mathf.Abs(offset.x) : -Mathf.Abs(offset.x);
        followScript.localOffset = offset;
        followScript.controllerTarget = interactor.transform;
    }
}
