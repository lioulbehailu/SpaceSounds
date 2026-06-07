using UnityEngine;
using System.Collections.Generic;

public class PlanetVisuals : MonoBehaviour
{
    [Header("Setup Renderers")]
    [SerializeField] private Renderer[] targetRenderers;

    [Header("Base Highlight Material")]
    [SerializeField] private Material baseHighlightMaterial;

    [Header("Docking Animation Settings")]
    [SerializeField] private float inflateScaleMultiplier = 1.15f;
    [SerializeField] private float animationSpeed = 5f;

    private Material runtimeMaterialInstance;
    private Vector3 originalScale;
    private Vector3 targetScale;

    private Color activeAudioColor = Color.clear;
    private bool isAudioColorActive = false;
    private bool isDockingActive = false;

    void Start()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (baseHighlightMaterial != null)
        {
            runtimeMaterialInstance = new Material(baseHighlightMaterial);
        }
    }

    void Update()
    {
        // Smoothly animate the scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
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
            if (isAudioColorActive)
            {
                ApplyColorOverlay(activeAudioColor); // Restore the playing track's color!
            }
            else
            {
                RemoveColorOverlay(); // Only completely strip if audio is muted (State 0)
            }
        }
    }

    public void SetAudioLoopColor(Color loopColor)
    {
        activeAudioColor = loopColor;
        isAudioColorActive = true;
        if (!isDockingActive)
        {
            ApplyColorOverlay(loopColor);
        }
    }

    public void ResetAudioColor()
    {
        isAudioColorActive = false;
        activeAudioColor = Color.clear;

        if (!isDockingActive)
        {
            RemoveColorOverlay();
        }
    }

    private void ApplyColorOverlay(Color newColor)
    {
        if (runtimeMaterialInstance != null)
        {
            runtimeMaterialInstance.SetColor("_BaseColor", newColor);
            runtimeMaterialInstance.EnableKeyword("_EMISSION");

            // Soft ambient glow calculations using the color's alpha channel
            float opacity = newColor.a;
            Color dimEmission = new Color(newColor.r, newColor.g, newColor.b) * opacity * 0.5f;
            runtimeMaterialInstance.SetColor("_EmissionColor", dimEmission);
        }

        foreach (Renderer ren in targetRenderers)
        {
            if (ren != null)
            {
                List<Material> mats = new List<Material>(ren.sharedMaterials);
                if (!mats.Contains(runtimeMaterialInstance))
                {
                    mats.Add(runtimeMaterialInstance);
                    ren.sharedMaterials = mats.ToArray();
                }
            }
        }
    }

    private void RemoveColorOverlay()
    {
        foreach (Renderer ren in targetRenderers)
        {
            if (ren != null)
            {
                List<Material> mats = new List<Material>(ren.sharedMaterials);
                if (mats.Contains(runtimeMaterialInstance))
                {
                    mats.Remove(runtimeMaterialInstance);
                    ren.sharedMaterials = mats.ToArray();
                }
            }
        }
    }

}
