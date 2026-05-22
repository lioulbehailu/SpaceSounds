using UnityEngine;
using FMODUnity;

public class PlanetAudioController : MonoBehaviour
{
    private StudioEventEmitter emitter;
    private int currentState = 0; // 0 = Off, 1 = Loop 1, 2 = Loop 2

    private PlanetVisuals planetVisuals;

    [Header("Loop Color Customization")]
    [SerializeField] private Color loop1Color = new Color(0f, 0.6f, 1f, 0.4f); // Transparent Blue
    [SerializeField] private Color loop2Color = new Color(1f, 0.3f, 0f, 0.4f); // Transparent Orange

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 15f;
    private bool isRotating = false;

    void Start()
    {
        emitter = GetComponent<StudioEventEmitter>();

        // Find the PlanetVisuals script sitting on this same root planet object
        planetVisuals = GetComponent<PlanetVisuals>();

        if (emitter != null)
        {
            emitter.SetParameter("PlanetState", 0);
            emitter.Play();
        }
    }

    void Update()
    {
        // Handle rotation if a loop is actively playing
        if (isRotating)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    public void TogglePlanetSound()
    {
        // Cycle: 0 -> 1 -> 2 -> 0
        currentState++;
        if (currentState > 2) currentState = 0;

        if (emitter != null)
        {
            emitter.SetParameter("PlanetState", (float)currentState);
            Debug.Log($"{gameObject.name} audio state is now: {currentState}");
        }

        // Apply visual updates based on our new audio loop state
        ApplyVisualState(currentState);
    }

    private void ApplyVisualState(int state)
    {
        switch (state)
        {
            case 0: // Muted / Off
                isRotating = false;
                if (planetVisuals != null)
                {
                    planetVisuals.ResetAudioColor(); // reset color only
                }
                break;

            case 1: // Loop 1 Active
                isRotating = true;
                if (planetVisuals != null)
                {
                    planetVisuals.SetAudioLoopColor(loop1Color); // change color
                }
                break;

            case 2: // Loop 2 Active
                isRotating = true;
                if (planetVisuals != null)
                {
                    planetVisuals.SetAudioLoopColor(loop2Color); // change color
                }
                break;
        }
    }
}
