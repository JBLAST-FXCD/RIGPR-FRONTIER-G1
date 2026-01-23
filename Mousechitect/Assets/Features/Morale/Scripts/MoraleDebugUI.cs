using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anthony - 23/01/26

/// <summary>
/// Temporary on-screen debug UI for morale.
/// Safe to delete later when real UI exists.
/// </summary>
public class MoraleDebugUI : MonoBehaviour
{
    private const float HAPPY_THRESHOLD = 0.25f;
    private const float SAD_THRESHOLD = -0.25f;

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

        float global_morale = morale_system.GetGlobalMorale();
        float morale_score = morale_system.GetMoraleScore();

        GUI.Label(new Rect(20, 90, 500, 20), "Global Morale: " + global_morale.ToString("0.00"));
        GUI.Label(new Rect(20, 110, 500, 20), "Morale Score (-1..1): " + morale_score.ToString("0.00"));
        GUI.Label(new Rect(20, 130, 500, 20), "Morale State: " + GetMoraleStateString(morale_score));

        GUI.Label(new Rect(20, 160, 500, 20), "Productivity: x" + morale_system.GetProductivityModifier().ToString("0.00"));
        GUI.Label(new Rect(20, 180, 500, 20), "Retention: x" + morale_system.GetRetentionModifier().ToString("0.00"));
        GUI.Label(new Rect(20, 200, 500, 20), "Arrival: x" + morale_system.GetArrivalChanceModifier().ToString("0.00"));
    }

    private string GetMoraleStateString(float morale_score)
    {
        if (morale_score >= HAPPY_THRESHOLD)
        {
            return "HAPPY";
        }

        if (morale_score <= SAD_THRESHOLD)
        {
            return "SAD";
        }

        return "NEUTRAL";
    }
}

