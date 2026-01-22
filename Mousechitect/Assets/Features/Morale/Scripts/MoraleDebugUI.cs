using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anthony - 21/01/26

/// <summary>
/// Temporary on-screen debug UI for morale.
/// Safe to delete later when real UI exists.
/// </summary>
public class MoraleDebugUI : MonoBehaviour
{
    [SerializeField] private MoraleSystem morale_system;

    private void OnGUI()
    {
        if (morale_system == null)
        {
            morale_system = FindObjectOfType<MoraleSystem>();
        }

        if (morale_system == null)
        {
            return;
        }

        GUI.Label(new Rect(20, 90, 500, 20), "Global Morale: " + morale_system.GetGlobalMorale().ToString("0.00"));
        GUI.Label(new Rect(20, 110, 500, 20), "Morale Score (-1..1): " + morale_system.GetMoraleScore().ToString("0.00"));
        GUI.Label(new Rect(20, 130, 500, 20), "Morale State: " + morale_system.GetMoraleState().ToString());

        GUI.Label(new Rect(20, 160, 500, 20), "Productivity: x" + morale_system.GetProductivityModifier().ToString("0.00"));
        GUI.Label(new Rect(20, 180, 500, 20), "Retention: x" + morale_system.GetRetentionModifier().ToString("0.00"));
        GUI.Label(new Rect(20, 200, 500, 20), "Arrival: x" + morale_system.GetArrivalModifier().ToString("0.00"));
    }
}
