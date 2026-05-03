using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceInteraction : MonoBehaviour
{
    #region Variables
    public float grabRange = 500f;                 // Maximum distance the grab ray can reach

    public float throwForce = 10f;                 // Force applied to the object when thrown into space

    private GameObject grabbedObject;              // The object currently being held by the player

    private Rigidbody grabbedRb;                   // The Rigidbody of the grabbed object (used to toggle physics)

    private SatelliteOnOrbit grabbedSatelliteScript;      // The Satellite script on the grabbed object (used to toggle orbital math)

    private GameObject currentZone;                // The snap zone the grabbed object is currently hovering over

    private LineRenderer beam;                     // The LineRenderer component used to draw the visual beam

    private float currentGrabDistance;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        // Cache the LineRenderer attached to this camera
        beam = GetComponent<LineRenderer>();

        // Hide all snap zones at startup so they only appear when grabbing
        ToggleAllSnapZones(false);
    }

    void Update()
    {
        // On the frame the left mouse button is pressed, try to grab an object
        if (Mouse.current.leftButton.wasPressedThisFrame) TryGrab();

        // On the frame the left mouse button is released, drop whatever we're holding
        if (Mouse.current.leftButton.wasReleasedThisFrame && grabbedObject != null) Release();

        if (Mouse.current.leftButton.isPressed)
        {
            // Show and update the visual beam while the button is held
            if (beam != null)
            {
                beam.enabled = true;
                DrawRay();
            }

            // If holding an object, move it to follow the mouse cursor in world space
            if (grabbedObject != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

                // Keep the object at the same depth it was grabbed at
                grabbedObject.transform.position = ray.GetPoint(currentGrabDistance);

                // Check if the object is hovering over a snap zone
                CheckForSnapZone();
            }
        }
        else
        {
            // Hide the beam and all snap zones when the button is not held
            if (beam != null) beam.enabled = false;
            ToggleAllSnapZones(false);
        }
    }
    #endregion

    #region Grabbing
    void TryGrab()
    {
        // Cast a ray from the camera through the mouse cursor position
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, grabRange))
        {
            // Only grab objects that have a Satellite component
            SatelliteOnOrbit sat = hit.collider.GetComponent<SatelliteOnOrbit>();
            if (sat != null)
            {
                grabbedObject = hit.collider.gameObject;

                // Record the grab depth so the object stays at the same distance while dragging
                currentGrabDistance = Vector3.Distance(transform.position, hit.point);

                grabbedRb = grabbedObject.GetComponent<Rigidbody>();
                grabbedSatelliteScript = sat;

                grabbedSatelliteScript.orbitPath = null;   // detach from orbit, SatelliteOnOrbit.Update() stops itself

                grabbedRb.isKinematic = false;             // let physics handle it while dragging

                // Reveal snap zones so the player knows where to drop the satellite
                ToggleAllSnapZones(true);
            }
        }
    }

    void Release()
    {
        // Re-enable physics on the released object
        grabbedRb.isKinematic = false;

        if (currentZone != null)
        {
            // SNAP: The object was dropped inside a snap zone — attach it to that orbit
            OrbitManager manager = currentZone.GetComponentInParent<OrbitManager>();
            if (manager != null)
            {
                grabbedRb.linearVelocity = Vector3.zero;
                grabbedRb.isKinematic = true;
                grabbedSatelliteScript.orbitPath = manager; // SatelliteOnOrbit.Update() picks this up next frame
                grabbedSatelliteScript.SnapToNearestPoint();
            }

            // Reset the zone back to its faint default color
            SetZoneColor(currentZone, new Color(1, 1, 1, 0.1f));
            currentZone = null;
        }
        else
        {
            // THROW: No zone — launch the object forward into space
            grabbedRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }

        // Hide all snap zones now that we've let go
        ToggleAllSnapZones(false);

        // Clear all references to the released object
        grabbedObject = null;
        grabbedRb = null;
    }
    #endregion

    #region Visual Beam
    void DrawRay()
    {
        // Start the beam from the bottom-center of the screen (simulates a hand position)
        Vector3 startPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.05f, 1f));
        beam.SetPosition(0, startPos);

        if (grabbedObject != null)
        {
            // If holding something, draw the beam toward the held object
            beam.SetPosition(1, grabbedObject.transform.position);
        }
        else
        {
            // If nothing is grabbed, shoot the beam forward into space toward the cursor
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            beam.SetPosition(1, ray.GetPoint(grabRange));
        }
    }
    #endregion

    #region Snap Zones
    void CheckForSnapZone()
    {
        // Cast a small sphere around the grabbed object to detect nearby snap zones
        Collider[] hits = Physics.OverlapSphere(grabbedObject.transform.position, 1.0f);
        bool foundZone = false;

        foreach (var hit in hits)
        {
            // Only care about colliders tagged as "SnapZone"
            if (hit.CompareTag("SnapZone"))
            {
                foundZone = true;

                // If this is a different zone than before, highlight it green
                if (currentZone != hit.gameObject)
                {
                    currentZone = hit.gameObject;
                    SetZoneColor(currentZone, new Color(0, 1, 0, 0.3f));
                }
            }
        }

        // If the object left the zone, reset the zone color and clear the reference
        if (!foundZone && currentZone != null)
        {
            SetZoneColor(currentZone, new Color(1, 1, 1, 0.1f));
            currentZone = null;
        }
    }

    void ToggleAllSnapZones(bool show)
    {
        // Find every object tagged "SnapZone" and show or hide its mesh
        GameObject[] zones = GameObject.FindGameObjectsWithTag("SnapZone");
        foreach (GameObject zone in zones)
        {
            MeshRenderer renderer = zone.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = show;
        }
    }

    void SetZoneColor(GameObject zone, Color color)
    {
        // Get the Renderer on the zone and change its material color
        Renderer ren = zone.GetComponent<Renderer>();
        if (ren != null)
        {
            ren.material.color = color;
        }
    }
    #endregion
}