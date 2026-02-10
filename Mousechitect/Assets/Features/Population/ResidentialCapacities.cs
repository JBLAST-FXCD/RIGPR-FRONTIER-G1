using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this script to any residential building prefab to tweak their capacity stats.
/// </summary>

public class ResidentialCapacities : MonoBehaviour
{
    [Header("Capacity Settings")]
    public int official_capacity = 6;
    public int visual_capacity = 3;

    private void Start()
    {
        if (PopulationManager.instance != null)
        {
            PopulationManager.instance.RegisterHousing(official_capacity, visual_capacity);
        }
    }

    private void OnDestroy()
    {
        if (PopulationManager.instance != null)
        {
            PopulationManager.instance.UnregisterHousing(official_capacity, visual_capacity);
        }
    }
}
