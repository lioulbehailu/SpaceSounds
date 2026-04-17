using UnityEngine;

public class Satellite : MonoBehaviour
{
    public OrbitManager orbitPath; // Drag your OrbitPath object here
    public float timeOffset = 0f;  // Allows satellites to be at different spots

    void Update()
    {
        if (orbitPath == null) return;
        
        // Ask the path where to be based on time + an offset
        transform.position = orbitPath.GetPositionAtTime(Time.time + timeOffset);
    }
}