using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anthony - 8/2/2026

/// <summary>
/// Morale adapter for city aesthetics.
/// Adds a small baseline morale gain per decoration, plus a synergy bonus when decorations
/// with overlapping tags exist within a given radius.
/// </summary>
public class DecorMoraleAdapter : MonoBehaviour, IMoraleContributor
{
    [Header("Scoring")]
    [SerializeField] private int synergy_radius_tiles = 4;
    [SerializeField] private float base_points_per_decor = 0.05f;
    [SerializeField] private float synergy_bonus_points = 0.10f;

    [Tooltip("Points required to reach score 1.0")]
    [SerializeField] private float points_for_max_score = 2.0f;

    public MORALE_CONTRIBUTOR_TYPE GetContributorType()
        => MORALE_CONTRIBUTOR_TYPE.TYPE_AESTHETICS;

    // Calculates an aesthetics score in the range 0-1
    // if decor has any nearby decor with overlapping tags it gains a bonus.
    public float GetContributionScore()
    {
        var reg = DecorRegistry.Instance;
        if (reg == null) return 0f;

        float points = 0f;

        foreach (var d in reg.GetAll())
        {
            if (d == null) continue;

            // small baseline reward
            points += base_points_per_decor * Mathf.Max(0.1f, d.base_value);

            // If decor has no tags, it cannot form synergies
            if (d.tags == DecorTag.None) continue;

            bool has_synergy = false;

            foreach (var other in reg.GetNearby(d.grid_cell, synergy_radius_tiles))
            {
                if (other == null || other == d) continue;

                // real radius check (circle) instead of square
                int dx = other.grid_cell.x - d.grid_cell.x;
                int dy = other.grid_cell.y - d.grid_cell.y;
                if (dx * dx + dy * dy > synergy_radius_tiles * synergy_radius_tiles)
                    continue;

                // Any overlapping tag counts as synergy
                if ((d.tags & other.tags) != DecorTag.None)
                {
                    has_synergy = true;
                    break;
                }
            }

            if (has_synergy)
                points += synergy_bonus_points;
        }

        // Convert points to a 0-1 contribution
        float score = Mathf.Clamp01(points / Mathf.Max(0.01f, points_for_max_score));
        Debug.Log($"[Morale-Decor] points={points:0.00} score={score:0.00}");
        return score;
    }
}

