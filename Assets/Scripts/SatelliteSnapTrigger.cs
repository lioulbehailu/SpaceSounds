using UnityEngine;
using FMODUnity;

public class SatelliteSnapTrigger : MonoBehaviour
{
    public string parameterName = "PlanetFilter";
    public float transitionSpeed = 2.0f;

    // We start at 0 (Open/Clear)
    private float targetValue = 0f;
    private float currentValue = 0f;
    
    private StudioEventEmitter activeEmitter;

    void Update()
    {
        // Smoothly slide the value
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * transitionSpeed);

        if (activeEmitter != null && activeEmitter.IsPlaying())
        {
            activeEmitter.EventInstance.setParameterByName(parameterName, currentValue);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "SnapZone")
        {
            activeEmitter = other.GetComponentInParent<StudioEventEmitter>();
            targetValue = 1f; // SNAP: Go to 1 (Closed/Muffled)
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "SnapZone")
        {
            targetValue = 0f; // RELEASE: Go to 0 (Open/Clear)
        }
    }
}