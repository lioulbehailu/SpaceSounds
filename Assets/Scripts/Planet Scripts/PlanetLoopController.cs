using FMODUnity;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PlanetLoopController : MonoBehaviour
{
    private StudioEventEmitter emitter;
    private int currentState = 0; // 0 = Off, 1 = Loop 1, 2 = Loop 2, 3 = Loop 3

    private PlanetVisualFeedback planetVisuals;

    [Header("Loop Settings")]
    [SerializeField] private Material loop0Material;
    [SerializeField] private Material loop1Material;
    [SerializeField] private Material loop2Material;
    [SerializeField] private Material loop3Material;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 15f;
    private bool isRotating = false;

    [Header("Trigger Haptics")]
    public HapticImpulsePlayer leftPlayer;
    public HapticImpulsePlayer rightPlayer;
    public float triggerAmplitude;
    public float triggerDuration;
    public float preselectAmplitude;
    public float preselectDuration;

    private XRBaseInteractable grabInteractable;
    private bool isReleasing = false;

    void Start()
    {
        emitter = GetComponent<StudioEventEmitter>();
        grabInteractable = GetComponent<XRBaseInteractable>();

        // Find the PlanetVisuals script sitting on this same root planet object
        planetVisuals = GetComponent<PlanetVisualFeedback>();

        if (emitter != null)
        {
            emitter.SetParameter("PlanetState", 0);
            emitter.Play();
        }

        ApplyVisualState(0);

    }

    void Update()
    {
        // Handle rotation if a loop is actively playing
        if (isRotating)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    public void OnPreselected()
    {
        if (isReleasing) return;

        if (leftPlayer != null && IsHoveredByController(leftPlayer))
        {
            leftPlayer.SendHapticImpulse(preselectAmplitude, preselectDuration);
        }
        else if (rightPlayer != null && IsHoveredByController(rightPlayer))
        {
            rightPlayer.SendHapticImpulse(preselectAmplitude, preselectDuration);
        }
    }

    public void TogglePlanetSound()
    {
        // Cycle: 0 -> 1 -> 2 -> 3 -> 0
        currentState++;
        if (currentState > 3) currentState = 0;

        if (emitter != null)
        {
            emitter.SetParameter("PlanetState", (float)currentState);
            Debug.Log($"{gameObject.name} audio state is now: {currentState}");
        }

        if (leftPlayer != null && (IsGrabbedByController(leftPlayer) || IsHoveredByController(leftPlayer)))
        {
            leftPlayer.SendHapticImpulse(triggerAmplitude, triggerDuration);
        }
        else if (rightPlayer != null && (IsGrabbedByController(rightPlayer) || IsHoveredByController(rightPlayer)))
        {
            rightPlayer.SendHapticImpulse(triggerAmplitude, triggerDuration);
        }

        // Apply visual updates based on our new audio loop state
        ApplyVisualState(currentState);
    }

    public void OnReleased()
    {
        isReleasing = true;

        // Invoke a reset at the end of the current frame to clear our block flag
        Invoke(nameof(ResetReleaseFlag), 0.05f);
    }

    private void ResetReleaseFlag()
    {
        isReleasing = false;
    }

    private void ApplyVisualState(int state)
    {
        switch (state)
        {
            case 0: // Muted / Off
                isRotating = false;
                if (planetVisuals != null)
                {
                    planetVisuals.SetAudioLoopTexture(loop0Material);
                }
                break;

            case 1: // Loop 1 Active
                isRotating = true;
                if (planetVisuals != null)
                {
                    planetVisuals.SetAudioLoopTexture(loop1Material);
                }
                break;

            case 2: // Loop 2 Active
                isRotating = true;
                if (planetVisuals != null)
                {
                    planetVisuals.SetAudioLoopTexture(loop2Material);
                }
                break;

            case 3: // Loop 3 Active
                isRotating = true;
                if (planetVisuals != null)
                {
                    planetVisuals.SetAudioLoopTexture(loop3Material);
                }
                break;
        }
    }

    #region Hover/Grab Verification
    // Checks if the specified controller player's transform matches the interactor hovering over this object
    private bool IsHoveredByController(HapticImpulsePlayer controllerPlayer)
    {
        if (grabInteractable == null || !grabInteractable.isHovered) return false;

        foreach (var interactor in grabInteractable.interactorsHovering)
        {
            if (interactor.transform.IsChildOf(controllerPlayer.transform) || controllerPlayer.transform.IsChildOf(interactor.transform))
            {
                return true;
            }
        }
        return false;
    }

    // Checks if the specified controller player's transform matches the interactor selecting this object
    private bool IsGrabbedByController(HapticImpulsePlayer controllerPlayer)
    {
        if (grabInteractable == null || !grabInteractable.isSelected) return false;

        foreach (var interactor in grabInteractable.interactorsSelecting)
        {
            if (interactor.transform.IsChildOf(controllerPlayer.transform) || controllerPlayer.transform.IsChildOf(interactor.transform))
            {
                return true;
            }
        }
        return false;
    }
    #endregion
}
