using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SatelliteFeedbackController : MonoBehaviour
{
    #region Public 
    [Header("Light Renderer")]
    public Renderer[] lightRenderer;

    [Header("Textures")]
    public Texture2D lightOffTexture;
    public Texture2D lightOnTexture;

    [Header("Blink")]
    public float blinkFrequency = 6f;      // How many times per second the light blinks while grabbed

    [Header("Hover/Grab Haptics")]
    public HapticImpulsePlayer leftPlayer;
    public HapticImpulsePlayer rightPlayer;
    public float grabAmplitude;
    public float grabDuration;
    public float preselectAmplitude = 0.2f;
    public float preselectDuration = 0.1f;
    #endregion

    #region State
    public enum LightState { Idle, Grabbed, InOrbit }
    public LightState currentState = LightState.Idle;
    #endregion

    #region Private
    private Material[] _mats;
    private float blinkTimer;
    private bool blinkIsOn;
    private XRBaseInteractable grabInteractable;
    private bool isReleasing = false;
    #endregion

    #region Setup
    void Awake()
    {
        grabInteractable = GetComponent<XRBaseInteractable>();

        if (lightRenderer == null)
        {
            Debug.LogError("[SatelliteLightController] No Renderer assigned!", this);
            enabled = false;
            return;
        }

        // Create a private material instance for each light so we don't affect shared assets
        _mats = new Material[lightRenderer.Length];
        for (int i = 0; i < lightRenderer.Length; i++)
        {
            _mats[i] = new Material(lightRenderer[i].material);
            lightRenderer[i].material = _mats[i];
        }

        ShowOff();
    }
    #endregion

    #region Update
    void Update()
    {
        if (currentState == LightState.Grabbed)
        {
            // Alternate between on and off at blinkFrequency times per second
            blinkTimer += Time.deltaTime;
            float halfPeriod = 0.5f / blinkFrequency;
            if (blinkTimer >= halfPeriod)
            {
                blinkTimer -= halfPeriod;
                blinkIsOn = !blinkIsOn;
                if (blinkIsOn) ShowOn();
                else ShowOff();
            }
        }
    }
    #endregion

    #region State Changes
    public void OnPreselected()
    {
        if (isReleasing) return;

        if (GetLeftPlayer() != null && IsHoveredByController(GetLeftPlayer()))
            GetLeftPlayer().SendHapticImpulse(preselectAmplitude, preselectDuration);
        else if (GetRightPlayer() != null && IsHoveredByController(GetRightPlayer()))
            GetRightPlayer().SendHapticImpulse(preselectAmplitude, preselectDuration);
    }
    public void OnGrabbed()
    {
        if (GetLeftPlayer() != null && IsGrabbedByController(GetLeftPlayer()))
            GetLeftPlayer().SendHapticImpulse(grabAmplitude, grabDuration);
        else if (GetRightPlayer() != null && IsGrabbedByController(GetRightPlayer()))
            GetRightPlayer().SendHapticImpulse(grabAmplitude, grabDuration);

        currentState = LightState.Grabbed;
        blinkTimer = 0f;
        blinkIsOn = false;
        ShowOff();
    }

    public void OnReleased()
    {
        // Turn on our release protection flag
        isReleasing = true;
        CancelInvoke(nameof(ResetReleaseFlag));
        Invoke(nameof(ResetReleaseFlag), 0.05f); // 50ms buffer window

        // If released inside an orbit snap zone, orbit state takes priority
        if (currentState == LightState.InOrbit) return;
        currentState = LightState.Idle;
        ShowOff();
    }

    private void ResetReleaseFlag()
    {
        isReleasing = false;
    }

    public void OnEnteredOrbit()
    {
        // Only enter orbit state if the satellite is not currently held
        if (currentState == LightState.Grabbed) return;
        currentState = LightState.InOrbit;
        ShowOn();
    }
    #endregion

    #region Visuals
    void ShowOn()
    {
        foreach (var mat in _mats)
            if (lightOnTexture != null) mat.mainTexture = lightOnTexture;
    }

    void ShowOff()
    {
        foreach (var mat in _mats)
            if (lightOffTexture != null) mat.mainTexture = lightOffTexture;
    }
    #endregion

    #region Hover/Grab Verfication
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

    private HapticImpulsePlayer GetLeftPlayer()
    {
        if (leftPlayer == null)
        {
            var left = GameObject.Find("Left Controller");
            if (left != null) leftPlayer = left.GetComponent<HapticImpulsePlayer>()
                                         ?? left.GetComponentInChildren<HapticImpulsePlayer>();
        }
        return leftPlayer;
    }

    private HapticImpulsePlayer GetRightPlayer()
    {
        if (rightPlayer == null)
        {
            var right = GameObject.Find("Right Controller");
            if (right != null) rightPlayer = right.GetComponent<HapticImpulsePlayer>()
                                           ?? right.GetComponentInChildren<HapticImpulsePlayer>();
        }
        return rightPlayer;
    }
}
