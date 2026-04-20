using UnityEngine;
using FMODUnity;
 // currently not working
public class SatelliteController : MonoBehaviour
{
    public string parameterName = "PlanetFilter"; // Match your FMOD parameter name!
    public float activeDistance = 20f;

    void Update()
    {
        // Find the planet by tag
        GameObject planet = GameObject.FindWithTag("Planet");

        if (planet != null)
        {
            if (planet.TryGetComponent<StudioEventEmitter>(out var emitter))
            {
                float dist = Vector3.Distance(transform.position, planet.transform.position);
                
// 0.0 when close (Muffled), 1.0 when far (Clear)
float intensity = Mathf.Clamp01(dist / activeDistance);
                // Send to FMOD
                if (emitter.IsPlaying())
                {
                    emitter.EventInstance.setParameterByName(parameterName, intensity);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activeDistance);
    }
}