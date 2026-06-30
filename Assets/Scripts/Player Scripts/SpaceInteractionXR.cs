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
    private SatelliteFeedbackController grabbedFeedbackController;

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor myInteractor;
    #endregion

    void Awake()
    {
        Transform nearFar = transform.Find("Near-Far Interactor");
        myInteractor = nearFar.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor>();
    }

    void OnEnable()
    {
        if (myInteractor != null)
        {
            myInteractor.selectEntered.AddListener(HandleSelectEntered);
            myInteractor.selectExited.AddListener(HandleSelectExited);
        }
    }

    void OnDisable()
    {
        if (myInteractor != null)
        {
            myInteractor.selectEntered.RemoveListener(HandleSelectEntered);
            myInteractor.selectExited.RemoveListener(HandleSelectExited);
        }
    }

    private void HandleSelectEntered(SelectEnterEventArgs args)
    {
        GameObject target = args.interactableObject.transform.gameObject;
        SatelliteOnOrbit satelliteCheck = target.GetComponent<SatelliteOnOrbit>();
        PlanetLoopController planetCheck = target.GetComponent<PlanetLoopController>();
        if (satelliteCheck != null) OnSatelliteGrabbed(args);
        else if (planetCheck != null) OnPlanetTriggered(args);
    }

    private void HandleSelectExited(SelectExitEventArgs args)
    {
        GameObject target = args.interactableObject.transform.gameObject;
        SatelliteOnOrbit satelliteCheck = target.GetComponent<SatelliteOnOrbit>();
        if (satelliteCheck != null) OnSatelliteReleased(args);
    }

    void Update()
    {
        if (grabbedObject != null)
        {
            // Path glow check
            if (PathGlowyLine.Instance != null)
            {
                bool near = IsSatelliteNearPath(grabbedObject);
                PathGlowyLine.Instance.SetGlowActive(near);
            }

            // Snap zone highlight check
            GameObject newZone = grabbedSatelliteFilterScript?.CurrentSnapZone;
            if (newZone != null && newZone != currentZone)
            {
                if (currentZone != null) TogglePlanetHighlight(currentZone, false);

                OrbitManager manager = newZone.GetComponentInParent<OrbitManager>();
                if (manager != null && grabbedFeedbackController != null)
                {
                    // Check if the planet's orbit dictionary already contains this specific enum type
                    if (!manager.CanAcceptSatellite(grabbedFeedbackController.satelliteType))
                    {
                        Debug.Log($"🚫 BLOCKED: Planet already contains a {grabbedFeedbackController.satelliteType} satellite.");
                        currentZone = null; // Deny entry status
                        return;
                    }
                }

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
        else
        {
            // No satellite grabbed, make sure glow is off
            if (PathGlowyLine.Instance != null)
                PathGlowyLine.Instance.SetGlowActive(false);
        }
    }

    public void OnSatelliteGrabbed(SelectEnterEventArgs args)
    {
        grabbedObject = args.interactableObject.transform.gameObject;
        grabbedRb = grabbedObject.GetComponent<Rigidbody>();
        grabbedSatelliteOnOrbitScript = grabbedObject.GetComponent<SatelliteOnOrbit>();
        grabbedSatelliteFilterScript = grabbedObject.GetComponent<SatelliteFilter>();
        grabbedFeedbackController = grabbedObject.GetComponent<SatelliteFeedbackController>(); // Get the feedback type cache

        if (grabbedSatelliteOnOrbitScript != null)
        {
            // If the satellite was pulled out of an active orbit path, free its type slot from that planet
            if (grabbedSatelliteOnOrbitScript.orbitPath != null && grabbedFeedbackController != null)
            {
                grabbedSatelliteOnOrbitScript.orbitPath.UnregisterSatellite(grabbedFeedbackController.satelliteType);
            }
            grabbedSatelliteOnOrbitScript.OnGrabbed();
        }

        grabbedRb.isKinematic = false;

        SatelliteFlightMover mover = grabbedObject.GetComponent<SatelliteFlightMover>();
        if (mover != null) mover.PauseFlight();

        if (PlanetGlowManager.Instance != null)
            PlanetGlowManager.Instance.SetSelectivePlanetsGlow(grabbedFeedbackController.satelliteType);
        else
            Debug.LogWarning("PlanetGlowManager instance is NULL!");
    }

    public void OnSatelliteReleased(SelectExitEventArgs args)
    {
        if (grabbedObject == null) return;

        SatelliteCheatSheetTrigger.CurrentlyOpen?.Close();

        if (PathGlowyLine.Instance != null)
            PathGlowyLine.Instance.SetGlowActive(false);

        if (PlanetGlowManager.Instance != null)
            PlanetGlowManager.Instance.SetAllPlanetsGlow(false);

        grabbedRb.isKinematic = false;

        if (currentZone != null)
        {
            OrbitManager manager = currentZone.GetComponentInParent<OrbitManager>();
            if (manager != null)
            {
                if (grabbedFeedbackController != null && manager.CanAcceptSatellite(grabbedFeedbackController.satelliteType))
                {
                    grabbedRb.linearVelocity = Vector3.zero;
                    grabbedRb.isKinematic = true;
                    grabbedSatelliteOnOrbitScript.orbitPath = manager;
                    grabbedSatelliteOnOrbitScript.SnapToNearestPoint();

                    // Lock the slot state on the orbit manager
                    manager.RegisterSatellite(grabbedFeedbackController.satelliteType, grabbedObject);
                }
                else
                {
                    // Check what's worth doing there !!!
                    grabbedSatelliteOnOrbitScript?.OnThrown();
                }
            }
            TogglePlanetHighlight(currentZone, false);
            currentZone = null;
        }
        else
        {
            grabbedSatelliteOnOrbitScript?.OnThrown();
        }

        grabbedObject = null;
        grabbedRb = null;
        grabbedSatelliteOnOrbitScript = null;
        grabbedSatelliteFilterScript = null;
    }

    public void OnPlanetTriggered(SelectEnterEventArgs args)
    {
        PlanetLoopController planetAudio = args.interactableObject.transform.GetComponent<PlanetLoopController>();
        if (planetAudio != null)
            planetAudio.TogglePlanetSound();

        SatelliteCheatSheetTrigger.CurrentlyOpen?.Close();
    }

    private bool IsSatelliteNearPath(GameObject sat)
    {
        SatelliteFlightPath path = FindAnyObjectByType<SatelliteFlightPath>();
        if (path == null) return false;

        SatelliteOnOrbit orbitScript = sat.GetComponent<SatelliteOnOrbit>();
        float snapDist = orbitScript != null ? orbitScript.GetSnapDistance() : 0.5f;

        for (int i = 0; i < path.GetWaypointCount() - 1; i++)
        {
            Vector3 a = path.GetPosition(i, 0f);
            Vector3 b = path.GetPosition(i, 1f);
            Vector3 closest = ClosestPointOnSegment(sat.transform.position, a, b);
            if (Vector3.Distance(sat.transform.position, closest) <= snapDist)
                return true;
        }
        return false;
    }

    private Vector3 ClosestPointOnSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(point - a, ab) / ab.sqrMagnitude;
        return a + ab * Mathf.Clamp01(t);
    }

    #region Snap Zone Logic
    void TogglePlanetHighlight(GameObject zone, bool turnOn)
    {
        PlanetVisualFeedback visuals = zone.GetComponentInParent<PlanetVisualFeedback>();
        Debug.Log("🎯 Trying to toggle planet highlight" + visuals.gameObject.name);
        if (visuals != null)
        {
            if (turnOn)
            {
                visuals.SetDockingInflation(true);
                visuals.SetGlowIntensity(true);
            }
            else
            {
                visuals.SetDockingInflation(false);
                visuals.SetGlowIntensity(false);
            }
        }
    }
    #endregion
}
