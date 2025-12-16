using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

//// Hani Hailston 15/12/2025

/// <summary>
/// This script will link the UI System to each building upgrade handler.
/// For now, functionality stops at capturing upgrade unlock commands and logging them.
/// </summary>

public class BuildingUnlockManager : MonoBehaviour
{
    public static BuildingUnlockManager instance;

    public void UnlockBuilding(string building_id)
    {
        Debug.Log($"[BuildingUnlockManager] Building ID '{building_id}' has been unlocked successfully.");
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
        }
    }
}
