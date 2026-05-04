using UnityEngine;
using FMODUnity;

public class PlanetAudioController : MonoBehaviour
{
    private StudioEventEmitter emitter;
    private int currentState = 0; // 0 = Off, 1 = Loop 1, 2 = Loop 2

    void Start()
    {
        // Automatically get the emitter on this specific planet
        emitter = GetComponent<StudioEventEmitter>();

        if (emitter != null)
        {
            // Set the initial state to 0 (silent)
            emitter.SetParameter("PlanetState", 0);
            emitter.Play();
        }
    }

    public void TogglePlanetSound()
    {
        // Cycle: 0 -> 1 -> 2 -> 0
        currentState++;
        if (currentState > 2) currentState = 0;

        if (emitter != null)
        {
            // Send the new state to FMOD
            emitter.SetParameter("PlanetState", (float)currentState);
            Debug.Log($"{gameObject.name} state is now: {currentState}");
        }
    }
}
