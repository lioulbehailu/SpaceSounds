using UnityEngine;
using FMODUnity;

public class SatelliteTool : MonoBehaviour
{
    [Header("Links")]
    public StudioEventEmitter targetPlanet; // Drag Jupiter here

    [Header("Settings")]
    public string fmodParameter = "PlanetProximity"; // Must match FMOD exactly
    public float activeDistance = 15f;

    void Update()
    {
        // 1. Safety check: does the planet exist and is the sound playing?
        if (targetPlanet == null || !targetPlanet.IsPlaying()) return;

        // 2. Calculate distance
        float dist = Vector3.Distance(transform.position, targetPlanet.transform.position);

// 0.0 when close (Muffled), 1.0 when far (Clear)
float intensity = Mathf.Clamp01(dist / activeDistance);

        // 4. Send the value to the specific planet instance
        targetPlanet.EventInstance.setParameterByName(fmodParameter, intensity);
    }

    // Visual guide in the Scene View
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activeDistance);
    }
}