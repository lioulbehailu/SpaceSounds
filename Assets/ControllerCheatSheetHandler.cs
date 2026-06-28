using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Attach to each controller GameObject (same level as SpaceInteractionXR).
// Handles cheat-sheet open/close via face buttons and remote interaction via grip/trigger.
public class ControllerCheatSheetHandler : MonoBehaviour
{
    [Tooltip("How far the ray can detect interactables for the cheat sheet")]
    [SerializeField] private float rayDistance = 20f;

    private XRBaseInteractor myInteractor;
    private InputAction faceButtonsAction;
    private InputAction selectAction;

    private SatelliteCheatSheetTrigger currentAimedTrigger;

    void Awake()
    {
        Transform nearFar = transform.Find("Near-Far Interactor");
        if (nearFar != null)
            myInteractor = nearFar.GetComponent<XRBaseInteractor>();

        bool isLeft = myInteractor != null && myInteractor.handedness == InteractorHandedness.Left;
        string hand = isLeft ? "{LeftHand}" : "{RightHand}";

        faceButtonsAction = new InputAction("CheatSheetFaceButtons");
        faceButtonsAction.AddBinding($"<XRController>{hand}/primaryButton");
        faceButtonsAction.AddBinding($"<XRController>{hand}/secondaryButton");
        faceButtonsAction.Enable();
        faceButtonsAction.performed += OnFaceButton;

        selectAction = new InputAction("CheatSheetSelect");
        selectAction.AddBinding($"<XRController>{hand}/{{GripButton}}");
        selectAction.AddBinding($"<XRController>{hand}/{{TriggerButton}}");
        selectAction.Enable();
        selectAction.performed += OnSelectPressed;
    }

    void OnDestroy()
    {
        faceButtonsAction.performed -= OnFaceButton;
        faceButtonsAction.Dispose();
        selectAction.performed -= OnSelectPressed;
        selectAction.Dispose();
    }

    void Update()
    {
        if (myInteractor == null) return;

        Transform ray = myInteractor.transform;
        currentAimedTrigger = Physics.Raycast(ray.position, ray.forward, out RaycastHit hit, rayDistance)
            ? hit.collider.GetComponentInParent<SatelliteCheatSheetTrigger>()
            : null;
    }

    private void OnFaceButton(InputAction.CallbackContext _)
    {
        if (currentAimedTrigger != null)
            currentAimedTrigger.Toggle(myInteractor);
        else
            SatelliteCheatSheetTrigger.CurrentlyOpen?.Close();
    }

    private void OnSelectPressed(InputAction.CallbackContext _)
    {
        var openSheet = SatelliteCheatSheetTrigger.CurrentlyOpen;
        if (openSheet == null) return;

        var interactable = openSheet.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable>();

        // If XRI is already hovering this object (in normal range), let XRI handle it
        if (interactable != null && myInteractor is IXRHoverInteractor hover &&
            interactable.interactorsHovering.Any(h => h == hover))
            return;

        // Remote planet interaction
        var planet = openSheet.GetComponent<PlanetLoopController>();
        if (planet != null)
        {
            planet.TogglePlanetSound();
            openSheet.Close();
            return;
        }

        // Remote satellite grab — route through XRI so SpaceInteractionXR picks it up normally
        if (interactable is UnityEngine.XR.Interaction.Toolkit.Interactables.IXRSelectInteractable selectable && myInteractor is IXRSelectInteractor selector)
        {
            interactable.interactionManager.SelectEnter(selector, selectable);
            openSheet.Close();
        }
    }
}
