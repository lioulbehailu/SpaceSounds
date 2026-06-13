using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpaceInteractionXR : MonoBehaviour
{
    #region Variables
    private GameObject grabbedObject;
    private Rigidbody grabbedRb;
    private SatelliteOnOrbit grabbedSatelliteOnOrbitScript;
    private SatelliteFilter grabbedSatelliteFilterScript;
    private GameObject currentZone;

    #endregion

    void Update()
    {
        if (grabbedObject != null)
        {
            GameObject newZone = grabbedSatelliteFilterScript?.CurrentSnapZone;

            if (newZone != null && newZone != currentZone)
            {
                if (currentZone != null) TogglePlanetHighlight(currentZone, false);
                currentZone = newZone;
                Debug.Log("🎯 SUCCESS: Entered SnapZone!");
                TogglePlanetHighlight(currentZone, true);
            }
            else if (newZone == null && currentZone != null)
            {
                Debug.Log("👋 LEFT ZONE: Resetting planet color.");
                TogglePlanetHighlight(currentZone, false);
                currentZone = null;
            }
        }
    }

    // Called when the VR hand selects (grabs) an object
    public void OnSatelliteGrabbed(SelectEnterEventArgs args)
    {
        grabbedObject = args.interactableObject.transform.gameObject;
        grabbedRb = grabbedObject.GetComponent<Rigidbody>();
        grabbedSatelliteOnOrbitScript = grabbedObject.GetComponent<SatelliteOnOrbit>();
        grabbedSatelliteFilterScript = grabbedObject.GetComponent<SatelliteFilter>();

        if (grabbedSatelliteOnOrbitScript != null)
        {
            grabbedSatelliteOnOrbitScript.OnGrabbed();
        }

        grabbedRb.isKinematic = false;
    }

    // Called when the VR hand deselects (releases) an object
    public void OnSatelliteReleased(SelectExitEventArgs args)
    {
        if (grabbedObject == null) return;

        grabbedRb.isKinematic = false;

        if (currentZone != null)
        {
            // SNAP TO ORBIT
            OrbitManager manager = currentZone.GetComponentInParent<OrbitManager>();
            if (manager != null)
            {
                grabbedRb.linearVelocity = Vector3.zero;
                grabbedRb.isKinematic = true;
                grabbedSatelliteOnOrbitScript.orbitPath = manager;
                grabbedSatelliteOnOrbitScript.SnapToNearestPoint();
            }
            TogglePlanetHighlight(currentZone, false);
            currentZone = null;
        }
        else
        {
            grabbedSatelliteOnOrbitScript?.OnThrown();
        }

        // Clear references
        grabbedObject = null;
        grabbedRb = null;
        grabbedSatelliteOnOrbitScript = null;
        grabbedSatelliteFilterScript = null;
    }

    // Called when the VR Controller ray points at a planet and triggers it
    public void OnPlanetTriggered(SelectEnterEventArgs args)
    {
        PlanetLoopController planetAudio = args.interactableObject.transform.GetComponent<PlanetLoopController>();
        if (planetAudio != null)
        {
            planetAudio.TogglePlanetSound();
        }
    }

    #region Snap Zone Logic
    void TogglePlanetHighlight(GameObject zone, bool turnOn)
    {
        // look to the parent for the visual manager script
        PlanetVisualFeedback visuals = zone.GetComponentInParent<PlanetVisualFeedback>();
        Debug.Log("🎯 Trying to toggle planet highlight" + visuals.gameObject.name);

        if (visuals != null)
        {
            if (turnOn)
            {
                // pass true to inflate
                visuals.SetDockingInflation(true);
                visuals.SetGlowIntensity(true);
            }
            else
            {
                // pass false to deflate
                visuals.SetDockingInflation(false);
                visuals.SetGlowIntensity(false); 
            }
        }
    }
    #endregion
}
