using UnityEngine;

public class Satellite : MonoBehaviour
{
    public OrbitManager orbitPath;
    public float timeOffset = 0f;

    void Update()
    {
        if (orbitPath == null) return;
        
        // Ask the path where to be based on time + an offset
        transform.position = orbitPath.GetPositionAtTime(Time.time + timeOffset);
    }

    // This runs the moment the script is turned back ON (when you drop it in a zone)
    void OnEnable()
    {
        // Stop any physical drifting when we start orbiting
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true; 
        }
    }    
}