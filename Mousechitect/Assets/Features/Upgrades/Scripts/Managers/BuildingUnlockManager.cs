using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

/// <summary>
/// This script will link the UI System to each building upgrade handler.
/// For now, functionality stops at capturing upgrade unlock commands and logging them.
/// </summary>
public class BuildingUnlockManager : MonoBehaviour
{
    public static BuildingUnlockManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void UnlockBuilding(string buildingID)
    {
        Debug.Log($"[BuildingUnlockManager] Building ID '{buildingID}' has been unlocked successfully.");
    }
}

