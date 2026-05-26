using UnityEngine;
using FMODUnity;

public class SatelliteFilter : MonoBehaviour
{
    // The name of the FMOD parameter to control (set in the FMOD Studio event)
    public string parameterName = "PlanetFilter";

    // How fast the filter slides between on and off
    public float transitionSpeed;

    // Where we want the filter to end up 
    private float targetValue = 0f;

    // Where the filter currently is 
    private float currentValue = 0f;

    // The FMOD emitter attached to the planet we're hovering over
    private StudioEventEmitter activeEmitter;

    // XRSpaceInteraction can read this to know if we're in a snap zone
    public GameObject CurrentSnapZone { get; private set; }

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

    private void OnTriggerEnter(Collider other)
    {
        // Fires when the satellite enters a snap zone collider
        if (other.name == "SnapZone")
        {
            CurrentSnapZone = other.gameObject; // expose it
            // Grab the FMOD emitter from the parent planet of this snap zone
            activeEmitter = other.GetComponentInParent<StudioEventEmitter>();

            // Tell the filter to turn on
            targetValue = 1f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Fires when the satellite leaves a snap zone collider
        if (other.name == "SnapZone")
        {
            CurrentSnapZone = null; // clear it
            // Tell the filter to turn off
            targetValue = 0f;
        }
    }
}
