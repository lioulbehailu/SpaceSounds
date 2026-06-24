using FMODUnity;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SatelliteFilter : MonoBehaviour
{
    #region FMOD Settings
    // The name of the FMOD parameter to control (set in the FMOD Studio event)
    public string parameterName = "PlanetFilter";
    public float transitionSpeed;
    #endregion

    #region Haptic Feedback
    [Header("SnapZone Haptics")]
    public HapticImpulsePlayer leftPlayer;
    public HapticImpulsePlayer rightPlayer;
    public float snapZoneEnterAmplitude;
    public float snapZoneEnterDuration;
    #endregion

    #region Internal State
    // Where we want the filter to end up 
    private float targetValue = 0f;
    private float currentValue = 0f;
    private StudioEventEmitter activeEmitter;
    private SatelliteFeedbackController lightController;
    private XRBaseInteractable grabInteractable;
    #endregion

    #region Snap Zone Reference
    public GameObject CurrentSnapZone { get; private set; }
    #endregion

    #region Connect SatelliteLightController
    void Awake()
    {
        lightController = GetComponent<SatelliteFeedbackController>();
        grabInteractable = GetComponent<XRBaseInteractable>();
    }
    #endregion

    #region Smooth Filter Transition
    void Update()
    {
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * transitionSpeed);
        if (activeEmitter != null && activeEmitter.IsPlaying())
        {
            activeEmitter.EventInstance.setParameterByName(parameterName, currentValue);
        }
    }
    #endregion

    #region Satellite entering Snap Zone Logic
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SnapZone"))
        {
            // Debounce check to prevent double vibrations if multiple colliders hit
            if (CurrentSnapZone == other.gameObject) return;

            CurrentSnapZone = other.gameObject; // ← expose it
            activeEmitter = other.GetComponentInParent<StudioEventEmitter>();
            targetValue = 1f;

            bool wasAlreadyInOrbit = (lightController != null && lightController.currentState == SatelliteFeedbackController.LightState.InOrbit);

            lightController.OnEnteredOrbit();

            if (wasAlreadyInOrbit) return;

            // Check which hand is actively grabbing the object for haptic feedback
            if (lightController.currentState == SatelliteFeedbackController.LightState.Grabbed)
            {
                if (GetLeftPlayer() != null && IsGrabbedByController(GetLeftPlayer()))
                    GetLeftPlayer().SendHapticImpulse(snapZoneEnterAmplitude, snapZoneEnterDuration);
                else if (GetRightPlayer() != null && IsGrabbedByController(GetRightPlayer()))
                    GetRightPlayer().SendHapticImpulse(snapZoneEnterAmplitude, snapZoneEnterDuration);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SnapZone"))
        {
            CurrentSnapZone = null;
            targetValue = 0f;
        }
    }
    #endregion

    #region Grab Verification
    // Checks if the specified player's transform matches the interactor currently selecting this object
    private bool IsGrabbedByController(HapticImpulsePlayer controllerPlayer)
    {
        if (grabInteractable == null || !grabInteractable.isSelected) return false;

        // Loop through all interactors currently grabbing this satellite
        foreach (var interactor in grabInteractable.interactorsSelecting)
        {
            // If the interactor object is on the same GameObject (or a child) as our assigned controller player
            if (interactor.transform.IsChildOf(controllerPlayer.transform) || controllerPlayer.transform.IsChildOf(interactor.transform))
            {
                return true;
            }
        }
        return false;
    }
    #endregion

    private HapticImpulsePlayer GetLeftPlayer()
    {
        if (leftPlayer == null)
        {
            var left = GameObject.Find("Left Controller");
            if (left != null) leftPlayer = left.GetComponent<HapticImpulsePlayer>()
                                         ?? left.GetComponentInChildren<HapticImpulsePlayer>();
        }
        return leftPlayer;
    }

    private HapticImpulsePlayer GetRightPlayer()
    {
        if (rightPlayer == null)
        {
            var right = GameObject.Find("Right Controller");
            if (right != null) rightPlayer = right.GetComponent<HapticImpulsePlayer>()
                                           ?? right.GetComponentInChildren<HapticImpulsePlayer>();
        }
        return rightPlayer;
    }
}
