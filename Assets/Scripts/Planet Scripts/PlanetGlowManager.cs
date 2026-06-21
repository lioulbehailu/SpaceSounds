using System.Collections.Generic;
using UnityEngine;

public class PlanetGlowManager : MonoBehaviour
{
    // Static instance allows any satellite to find this manager instantly
    public static PlanetGlowManager Instance { get; private set; }

    // List tracking every active planet script in the scene
    private List<PlanetGlow> activePlanets = new List<PlanetGlow>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        foreach (var p in FindObjectsByType<PlanetGlow>(FindObjectsSortMode.None))
            RegisterPlanet(p);
    }

    // Planets automatically sign themselves up when they spawn
    public void RegisterPlanet(PlanetGlow planet)
    {
        if (!activePlanets.Contains(planet))
        {
            activePlanets.Add(planet);
        }
    }

    public void UnregisterPlanet(PlanetGlow planet)
    {
        if (activePlanets.Contains(planet))
        {
            activePlanets.Remove(planet);
        }
    }

    // The core function called by any satellite
    public void SetAllPlanetsGlow(bool shouldGlow)
    {
        Debug.Log($"SetAllPlanetsGlow({shouldGlow}) — planet count: {activePlanets.Count}");

        foreach (PlanetGlow planet in activePlanets)
        {
            if (planet != null)
            {
                planet.ToggleGlow(shouldGlow);
            }
        }
    }
}
