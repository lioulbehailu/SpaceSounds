using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.InputSystem;
#endif

public class ResetAllTrigger : MonoBehaviour
{
    [Header("Hold Requirement")]
    [SerializeField] private float requiredHoldTime = 3.0f;
    private float holdTimer = 0f;

    void Update()
    {
        bool comboActive = CheckResetCombo();

        if (comboActive)
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= requiredHoldTime)
            {
                holdTimer = 0f;
                PerformReset();
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private bool CheckResetCombo()
    {
#if UNITY_EDITOR
        // Editor test shortcut: hold R
        if(Keyboard.current.digit9Key.isPressed || Keyboard.current.numpad9Key.isPressed)
        {
            return true;
        }
#endif

        // Left controller: trigger (index or hand) + top face button (X or Y = Button.Three/Four)
        bool leftTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch) > 0.7f ||
                            OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0.7f;
        bool leftFaceButton = OVRInput.Get(OVRInput.Button.Three) || OVRInput.Get(OVRInput.Button.Four);

        // Right controller: trigger (index or hand) + top face button (A or B = Button.One/Two)
        bool rightTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch) > 0.7f ||
                             OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0.7f;
        bool rightFaceButton = OVRInput.Get(OVRInput.Button.One) || OVRInput.Get(OVRInput.Button.Two);

        return leftTrigger && leftFaceButton && rightTrigger && rightFaceButton;
    }

    private void PerformReset()
    {
        Debug.Log("[ResetAllTrigger] Four-button combo held for 3s. Resetting scene state...");

        // 1. Clear all active satellites registered in OrbitManagers and destroy them
        OrbitManager[] orbitManagers = Object.FindObjectsByType<OrbitManager>(FindObjectsSortMode.None);
        foreach (var manager in orbitManagers)
        {
            manager.ClearAllSatellites();
        }

        // 2. Fallback sweep to catch any lingering orbiting satellites
        SatelliteFeedbackController[] satellites = Object.FindObjectsByType<SatelliteFeedbackController>(FindObjectsSortMode.None);
        foreach (var satellite in satellites)
        {
            if (satellite != null && satellite.currentState == SatelliteFeedbackController.LightState.InOrbit)
            {
                Destroy(satellite.gameObject);
            }
        }

        // 3. Reset all planets to loop0 and turn off their orbit glow rings
        PlanetLoopController[] planets = Object.FindObjectsByType<PlanetLoopController>(FindObjectsSortMode.None);
        foreach (var planet in planets)
        {
            planet.ResetToLoop0();

            // Turn off the orbit glow ring visuals
            PlanetVisualFeedback visuals = planet.GetComponent<PlanetVisualFeedback>();
            if (visuals != null)
            {
                visuals.SetDockingInflation(false);
                visuals.SetGlowIntensity(false);
            }

            // Reset individual snap zone counters
            SnapZoneCounter zone = planet.GetComponentInChildren<SnapZoneCounter>();
            if (zone != null)
            {
                zone.ForceReset();
            }
        }

        Debug.Log("[ResetAllTrigger] Scene completely reset: filters cleared, orbits unlit, planets set to loop0.");
    }
}
