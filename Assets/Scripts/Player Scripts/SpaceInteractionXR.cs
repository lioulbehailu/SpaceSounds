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
        // If holding an object in VR, check if it's near a snap zone
        if (grabbedObject != null)
        {
            CheckForSnapZone();
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
            // THROW INTO SPACE (Uses the controller's forward direction)
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
    void CheckForSnapZone()
    {
        // Try increasing 1.0f to 5.0f to give yourself a wider target for testing
        Collider[] hits = Physics.OverlapSphere(grabbedObject.transform.position, 5.0f);
        bool foundZone = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("SnapZone"))
            {
                foundZone = true;

                if (currentZone != hit.gameObject)
                {
                    if (currentZone != null) TogglePlanetHighlight(currentZone, false);

                    currentZone = hit.gameObject;
                    Debug.Log("🎯 SUCCESS: Entered SnapZone! Attempting to highlight planet.");
                    TogglePlanetHighlight(currentZone, true);
                }
            }
        }

        if (!foundZone && currentZone != null)
        {
            Debug.Log("👋 LEFT ZONE: Resetting planet color.");
            TogglePlanetHighlight(currentZone, false);
            currentZone = null;
        }
    }

    void TogglePlanetHighlight(GameObject zone, bool turnOn)
    {
        // look to the look parent for the visual manager script
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
