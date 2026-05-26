using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRSpaceInteraction : MonoBehaviour
{
    #region Variables
    public float throwForce = 10f;

    private GameObject grabbedObject;
    private Rigidbody grabbedRb;
    private SatelliteOnOrbit grabbedSatelliteScript;
    private GameObject currentZone;
    #endregion

    void Update()
    {
        if (grabbedObject != null)
        {
            SatelliteFilter filter = grabbedObject.GetComponent<SatelliteFilter>();
            if (filter != null)
            {
                GameObject newZone = filter.CurrentSnapZone;

                // Entered a zone
                if (newZone != null && newZone != currentZone)
                {
                    if (currentZone != null) TogglePlanetHighlight(currentZone, false);
                    currentZone = newZone;
                    TogglePlanetHighlight(currentZone, true);
                }
                // Left a zone
                else if (newZone == null && currentZone != null)
                {
                    TogglePlanetHighlight(currentZone, false);
                    currentZone = null;
                }
            }
        }
    }

    // Called when the VR hand selects (grabs) an object
    public void OnSatelliteGrabbed(SelectEnterEventArgs args)
    {
        grabbedObject = args.interactableObject.transform.gameObject;
        grabbedRb = grabbedObject.GetComponent<Rigidbody>();
        grabbedSatelliteScript = grabbedObject.GetComponent<SatelliteOnOrbit>();

        if (grabbedSatelliteScript != null)
        {
            grabbedSatelliteScript.orbitPath = null; // Detach from orbit
        }

        grabbedRb.isKinematic = false; // Let VR hand physics move it
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
                grabbedSatelliteScript.orbitPath = manager;
                grabbedSatelliteScript.SnapToNearestPoint();
            }
            TogglePlanetHighlight(currentZone, false);
            currentZone = null;
        }
        else
        {
            // THROW INTO SPACE
            grabbedRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }

        // Clear references
        grabbedObject = null;
        grabbedRb = null;
        grabbedSatelliteScript = null;
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
        PlanetVisuals visuals = zone.GetComponentInParent<PlanetVisuals>();
        Debug.Log("🎯 Trying to toggle planet highlight" + visuals.gameObject.name);

        if (visuals != null)
        {
            if (turnOn)
            {
                Color ambientGreen = new Color(0f, 1f, 0.2f, 0.4f);
                visuals.SetDockingInflation(true, ambientGreen);
            }
            else
            {
                visuals.SetDockingInflation(false, Color.black);
            }
        }
    }
    #endregion
}
