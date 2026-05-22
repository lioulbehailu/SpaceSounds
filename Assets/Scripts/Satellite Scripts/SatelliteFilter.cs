using UnityEngine;
using FMODUnity;

public class SatelliteFilter : MonoBehaviour
{
    #region FMOD Settings
    // The name of the FMOD parameter to control (set in the FMOD Studio event)
    public string parameterName = "PlanetFilter";

    // How fast the filter slides between on and off
    public float transitionSpeed;
    #endregion

    #region Internal State
    // Where we want the filter to end up 
    private float targetValue = 0f;

    // Where the filter currently is 
    private float currentValue = 0f;

    // The FMOD emitter attached to the planet we're hovering over
    private StudioEventEmitter activeEmitter;

    // Reference for SatelliteLightController script
    private SatelliteLightController lightController;
    #endregion

    #region Connect SatelliteLightController
    void Awake()
    {
        lightController = GetComponent<SatelliteLightController>(); 
    }
    #endregion

    #region Smooth Filter Transition 
    void Update()
    {
        // Smoothly slide currentValue toward targetValue each frame instead of snapping instantly
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * transitionSpeed);

        // Only update the FMOD parameter if there's an active emitter and it's actually playing
        if (activeEmitter != null && activeEmitter.IsPlaying())
        {
            activeEmitter.EventInstance.setParameterByName(parameterName, currentValue);
        }
    }
    #endregion

    #region Satellite entering Snap Zone Logic
    private void OnTriggerEnter(Collider other)
    {
        // Fires when the satellite enters a snap zone collider
        if (other.name == "SnapZone")
        {
            // Grab the FMOD emitter from the parent planet of this snap zone
            activeEmitter = other.GetComponentInParent<StudioEventEmitter>();

            // Tell the filter to turn on
            targetValue = 1f;
            lightController.OnEnteredOrbit();  // change light visuals
        }
    }
    #endregion
}
