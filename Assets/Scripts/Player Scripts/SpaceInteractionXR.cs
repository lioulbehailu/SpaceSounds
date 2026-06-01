using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpaceInteractionXR : MonoBehaviour
{
    #region Variables
    public float throwForce = 10f;

    private GameObject grabbedObject;
    private Rigidbody grabbedRb;
    private SatelliteOnOrbit grabbedSatelliteScript;
    private SatelliteFilter grabbedSatelliteFilter;
    #endregion

    void Update()
    {
        if (grabbedObject == null || grabbedSatelliteFilter == null)
            return;

        GameObject zone = grabbedSatelliteFilter.CurrentSnapZone;

        if (zone != null)
        {
            Debug.Log("🎯 In SnapZone");
            TogglePlanetHighlight(zone, true);
        }
    }

    // Called when the VR hand selects (grabs) an object
    public void OnSatelliteGrabbed(SelectEnterEventArgs args)
    {
        grabbedObject = args.interactableObject.transform.gameObject;
        grabbedRb = grabbedObject.GetComponent<Rigidbody>();
        grabbedSatelliteScript = grabbedObject.GetComponent<SatelliteOnOrbit>();
        grabbedSatelliteFilter = grabbedObject.GetComponent<SatelliteFilter>();

        if (grabbedSatelliteScript != null)
        {
            grabbedSatelliteScript.orbitPath = null;
        }

        grabbedRb.isKinematic = false;
    }

    // Called when the VR hand deselects (releases) an object
    public void OnSatelliteReleased(SelectExitEventArgs args)
    {
        if (grabbedObject == null) return;

        grabbedRb.isKinematic = false;

        if (grabbedSatelliteFilter != null &&
            grabbedSatelliteFilter.CurrentSnapZone != null)
        {
            OrbitManager manager =
                grabbedSatelliteFilter.CurrentSnapZone.GetComponentInParent<OrbitManager>();

            grabbedRb.linearVelocity = Vector3.zero;
            grabbedRb.isKinematic = true;
            grabbedSatelliteScript.orbitPath = manager;
            grabbedSatelliteScript.SnapToNearestPoint();
        }

        // Clear references
        grabbedObject = null;
        grabbedRb = null;
        grabbedSatelliteScript = null;
        grabbedSatelliteFilter = null;
    }

    // Called when the VR Controller ray points at a planet and triggers it
    public void OnPlanetTriggered(SelectEnterEventArgs args)
    {
        PlanetAudioController planetAudio = args.interactableObject.transform.GetComponent<PlanetAudioController>();
        if (planetAudio != null)
        {
            planetAudio.TogglePlanetSound();
        }
    }

    #region Snap Zone Logic
    void TogglePlanetHighlight(GameObject zone, bool turnOn)
    {
        // look to the parent for the visual manager script
        PlanetVisuals visuals = zone.GetComponentInParent<PlanetVisuals>();
        Debug.Log("🎯 Trying to toggle planet highlight" + visuals.gameObject.name);

        if (visuals != null)
        {
            if (turnOn)
            {
                // pass true to inflate with ambient color
                Color ambientGreen = new Color(0f, 1f, 0.2f, 0.4f);
                visuals.SetDockingInflation(true, ambientGreen);
            }
            else
            {
                // pass false to deflate and remove the ambient color
                visuals.SetDockingInflation(false, Color.black);
            }
        }
    }
    #endregion
}
