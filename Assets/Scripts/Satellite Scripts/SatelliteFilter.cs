using UnityEngine;
using FMODUnity;

public class SatelliteFilter : MonoBehaviour
{
    #region FMOD Settings
    public string parameterName = "PlanetFilter";
    public float transitionSpeed;
    #endregion

    #region Internal State
    private float targetValue = 0f;
    private float currentValue = 0f;
    private StudioEventEmitter activeEmitter;
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
        currentValue = Mathf.Lerp(currentValue, targetValue, Time.deltaTime * transitionSpeed);

        if (activeEmitter != null && activeEmitter.IsPlaying())
        {
            activeEmitter.EventInstance.setParameterByName(parameterName, currentValue);
        }
    }
    #endregion

    #region Snap Zone Callbacks (called by XRSpaceInteraction)
    public void OnEnteredSnapZone(GameObject zone)
    {
        activeEmitter = zone.GetComponentInParent<StudioEventEmitter>();
        targetValue = 1f;
        lightController?.OnEnteredOrbit();
    }

    public void OnExitedSnapZone()
    {
        targetValue = 0f;
        activeEmitter = null;
    }
    #endregion
}