using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceInteraction : MonoBehaviour
{
    public float grabRange = 500f;
    public float throwForce = 10f;
    public float maxRayDistance = 500f; // Very long ray like VR

    private GameObject grabbedObject;
    private Rigidbody grabbedRb;
    private Satellite grabbedSatelliteScript;
    
    private GameObject currentZone; 
    private LineRenderer beam;
    private float currentGrabDistance;


    void Start() {
        beam = GetComponent<LineRenderer>();
        // Hide all zones at the very beginning
        ToggleAllSnapZones(false);
    }    

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame) TryGrab();

        if (Mouse.current.leftButton.wasReleasedThisFrame && grabbedObject != null) Release();

        if (Mouse.current.leftButton.isPressed)
        {
            if (beam !=null)
            {
                beam.enabled = true;
                DrawRay();
            }

            // If we are holding something, update its position and check for zones
            if (grabbedObject != null)
            {
                Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
                grabbedObject.transform.position = ray.GetPoint(currentGrabDistance);
                CheckForSnapZone();
            }
        }
        else
        {
            if (beam != null) beam.enabled = false;
            ToggleAllSnapZones(false);
        }
    }

    void DrawRay()
    {
        // Origin: Bottom-middle of the viewport (the "hand" position)
        Vector3 startPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.05f, 1f));
        beam.SetPosition(0, startPos);

        if (grabbedObject != null)
        {
            // If holding something, the ray ends at the object
            beam.SetPosition(1, grabbedObject.transform.position);
        }
        else
        {
            // If just pointing, the ray shoots forward into space
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            beam.SetPosition(1, ray.GetPoint(maxRayDistance));
        }
    }

    void CheckForSnapZone()
    {
        // Shoot a small invisible sphere from the grabbed object to find the zone
        Collider[] hits = Physics.OverlapSphere(grabbedObject.transform.position, 1.0f);
        bool foundZone = false;

        foreach (var hit in hits)
        {
            if (hit.CompareTag("SnapZone")) // Matches your tag name!
            {
                foundZone = true;
                if (currentZone != hit.gameObject)
                {
                    // If we enter a new zone, turn it a highlight color (e.g., Green)
                    currentZone = hit.gameObject;
                    SetZoneColor(currentZone, new Color(0, 1, 0, 0.3f)); 
                }
            }
        }

        // If we move out of the zone, turn it back to a faint color (e.g., White/Transparent)
        if (!foundZone && currentZone != null)
        {
            SetZoneColor(currentZone, new Color(1, 1, 1, 0.1f));
            currentZone = null;
        }
    }

    void ToggleAllSnapZones(bool show)
    {
        // Find all objects tagged "snapzone" and hide/show their mesh
        GameObject[] zones = GameObject.FindGameObjectsWithTag("SnapZone");
        foreach (GameObject zone in zones)
        {
            MeshRenderer renderer = zone.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = show;
        }
    }
    void Release()
    {
        grabbedRb.isKinematic = false;

        if (currentZone != null)
        {
            // SNAP: Connect the satellite to the planet's orbit
            OrbitManager manager = currentZone.GetComponentInParent<OrbitManager>();
            if (manager != null)
            {
                grabbedSatelliteScript.orbitPath = manager;
                grabbedSatelliteScript.enabled = true; // Start the orbital math!
            }
            
            // Reset zone color after dropping
            SetZoneColor(currentZone, new Color(1, 1, 1, 0.1f));
            currentZone = null;
        }
        else
        {
            // THROW: Just drift into space
            grabbedRb.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }

        // Hide zones again after release
        ToggleAllSnapZones(false);

        grabbedObject = null;
        grabbedRb = null;
    }

    void SetZoneColor(GameObject zone, Color color)
    {
        Renderer ren = zone.GetComponent<Renderer>();
        if (ren != null)
        {
            ren.material.color = color;
        }
    }

    void TryGrab()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, grabRange))
        {
            Satellite sat = hit.collider.GetComponent<Satellite>();
            if (sat != null)
            {
                grabbedObject = hit.collider.gameObject;
                currentGrabDistance = Vector3.Distance(transform.position, hit.point);
                
                grabbedRb = grabbedObject.GetComponent<Rigidbody>();
                grabbedSatelliteScript = sat;

                grabbedSatelliteScript.enabled = false;
                grabbedRb.isKinematic = true;

                // Show zones only when we successfully grab something
                ToggleAllSnapZones(true);
            }
        }
    }
}