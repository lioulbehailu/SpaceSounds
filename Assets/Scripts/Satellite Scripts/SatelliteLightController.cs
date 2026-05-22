using UnityEngine;

public class SatelliteLightController : MonoBehaviour
{
    #region Public 
    [Header("Light Renderer")]
    public Renderer[] lightRenderer;

    [Header("Textures")]
    public Texture2D lightOffTexture;
    public Texture2D lightOnTexture;

    [Header("Blink")]
    public float blinkFrequency = 6f;      // How many times per second the light blinks while grabbed
    #endregion

    #region State
    public enum LightState { Idle, Grabbed, InOrbit }
    private LightState currentState = LightState.Idle;
    #endregion

    #region Private
    private Material[] _mats;
    private float blinkTimer;
    private bool blinkIsOn;
    #endregion

    #region Setup
    void Awake()
    {
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
    public void OnGrabbed()
    {
        currentState = LightState.Grabbed;
        blinkTimer = 0f;
        blinkIsOn = false;
        ShowOff();
    }

    public void OnReleased()
    {
        // If released inside an orbit snap zone, orbit state takes priority
        if (currentState == LightState.InOrbit) return;
        currentState = LightState.Idle;
        ShowOff();
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
}
