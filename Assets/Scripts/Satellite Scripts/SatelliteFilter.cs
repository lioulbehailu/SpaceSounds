using UnityEngine;
using FMODUnity;

public class SatelliteFilter : MonoBehaviour
{
    #region FMOD Settings
    // The name of the FMOD parameter to control (set in the FMOD Studio event)
    public string parameterName = "PlanetFilter";
    public float transitionSpeed;
    #endregion

    #region Internal State
    // Where we want the filter to end up 
    private float targetValue = 0f;
    private float currentValue = 0f;
    private StudioEventEmitter activeEmitter;
    private SatelliteLightController lightController;
    #endregion

    #region Snap Zone Reference
    public GameObject CurrentSnapZone { get; private set; }
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
            CurrentSnapZone = other.gameObject; // ← expose it
            activeEmitter = other.GetComponentInParent<StudioEventEmitter>();
            targetValue = 1f;
            lightController.OnEnteredOrbit();
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
}
