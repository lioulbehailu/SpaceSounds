using UnityEngine;
using System.Collections.Generic;

public class PlanetVisualFeedback : MonoBehaviour
{
    [Header("Setup Renderers")]
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Base Highlight Material")]
    [SerializeField] private Material baseHighlightMaterial;

    [Header("Orbit Glow Material")]
    [SerializeField] private Material orbitGlowMaterial; // 👈 Assign your OrbitGlow material preset here

    [Header("Inflation Target Settings")]
    [SerializeField] private Transform inflationTarget;

    [Header("Animation Settings")]
    [SerializeField] private float inflateScaleMultiplier = 1.15f;
    [SerializeField] private float animationSpeed = 5f;
    [SerializeField] private float intensiveGlowMultiplier = 3.0f; // How much to boost the emission HDR intensity

    [Header("SnapZone Reference")]
    [SerializeField] private SnapZoneCounter snapZone;

    private Material runtimeOrbitMaterial;
    private Material runtimeMaterialInstance;
    private Vector3 originalScale;
    private Vector3 targetScale;

    private Color activeAudioColor = Color.clear;
    private bool isAudioColorActive = false;
    private bool isDockingActive = false;
    private Material originalMaterial;

    // OrbitGlow cache variables
    private Color originalGlowColor;
    private Color targetGlowColor;

    void Start()
    {
        originalScale = inflationTarget.localScale;
        targetScale = originalScale;

        // 1. Handle base highlight material as before
        if (baseHighlightMaterial != null)
        {
            runtimeMaterialInstance = new Material(baseHighlightMaterial);
        }

        // 2. Handle Orbit material
        if (orbitGlowMaterial != null)
        {
            // Create the instance
            runtimeOrbitMaterial = new Material(orbitGlowMaterial);

            Renderer orbitRenderer = GetComponent<Renderer>(); 
            if (orbitRenderer != null)
            {
                orbitRenderer.material = runtimeOrbitMaterial;
            }

            originalGlowColor = runtimeOrbitMaterial.GetColor("_EmissionColor");
            targetGlowColor = originalGlowColor;
            runtimeOrbitMaterial.SetColor("_EmissionColor", Color.black);
        }

        if (targetRenderers != null && targetRenderers.Length > 0 && targetRenderers[0] != null)
            originalMaterial = targetRenderers[0].sharedMaterials[0];
    }

    void Update()
    {
        inflationTarget.localScale = Vector3.Lerp(inflationTarget.localScale, targetScale, Time.deltaTime * animationSpeed);

        Color finalGlowTarget = (snapZone.IsOccupied || isDockingActive) ? targetGlowColor : Color.black;

        if (runtimeOrbitMaterial != null) // Check the runtime instance
        {
            Color currentGlow = runtimeOrbitMaterial.GetColor("_EmissionColor");
            Color nextGlow = Color.Lerp(currentGlow, finalGlowTarget, Time.deltaTime * animationSpeed);
            runtimeOrbitMaterial.SetColor("_EmissionColor", nextGlow);
        }
    }

    public void SetDockingInflation(bool inflate)
    {
        isDockingActive = inflate;

        if (inflate)
        {
            targetScale = originalScale * inflateScaleMultiplier; // Inflate
        }
        else
        {
            targetScale = originalScale; // Deflate
        }
    }

    // 👈 New method to increase/decrease OrbitGlow emission intensity
    public void SetGlowIntensity(bool intensive)
    {
        if (runtimeOrbitMaterial == null) return;

        if (intensive)
        {
            // Boost the original emission color by your multiplier
            targetGlowColor = originalGlowColor * intensiveGlowMultiplier;
        }
        else
        {
            // Return to the original asset value
            targetGlowColor = originalGlowColor;
        }
    }

    public void SetAudioLoopTexture(Material loopMaterial)
    {
        isAudioColorActive = false;
        activeAudioColor = Color.clear;

        foreach (Renderer ren in targetRenderers)
        {
            if (ren == null) continue;
            var mats = ren.materials;  
            mats[0] = loopMaterial;
            ren.materials = mats;      
        }
    }

    void OnDestroy()
    {
        if (runtimeMaterialInstance != null)
            Destroy(runtimeMaterialInstance);

        if (runtimeOrbitMaterial != null)
            Destroy(runtimeOrbitMaterial);
    }
}
