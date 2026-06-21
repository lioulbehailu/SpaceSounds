using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SatelliteCheatSheetTrigger : MonoBehaviour
{
    [Header("Satellite Cheat Sheet Graphics Asset")]
    // Drop your specific satellite's purple sprite child asset here
    [SerializeField] private Sprite satelliteCheatSheetSprite;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable interactable;

    void Awake()
    {
        interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();
    }

    void OnEnable()
    {
        if (interactable == null) return;
        interactable.selectEntered.AddListener(OnPlayerGrabbed);
        interactable.selectExited.AddListener(OnPlayerReleased);
    }

    void OnDisable()
    {
        if (interactable == null) return;
        interactable.selectEntered.RemoveListener(OnPlayerGrabbed);
        interactable.selectExited.RemoveListener(OnPlayerReleased);
    }

    private void OnPlayerGrabbed(SelectEnterEventArgs args)
    {
        if (UiManager.Instance == null) return;

        GameObject canvasObj = UiManager.Instance.GetCanvasObject();
        SmoothFollowUI followScript = UiManager.Instance.GetFollowScript();

        if (canvasObj == null || followScript == null) return;

        // Inject this satellite's unique pre-made sprite texture into the UI system
        followScript.UpdateSpriteContent(satelliteCheatSheetSprite);

        Transform interactingHand = args.interactorObject.transform;
        followScript.controllerTarget = interactingHand;

        Vector3 initialTargetPos = interactingHand.TransformPoint(followScript.localOffset);
        canvasObj.transform.position = initialTargetPos;
        canvasObj.SetActive(true);
    }

    private void OnPlayerReleased(SelectExitEventArgs args)
    {
        if (UiManager.Instance == null) return;

        GameObject canvasObj = UiManager.Instance.GetCanvasObject();
        SmoothFollowUI followScript = UiManager.Instance.GetFollowScript();

        if (canvasObj != null) canvasObj.SetActive(false);
        if (followScript != null) followScript.controllerTarget = null;
    }
}