using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Iain Benner 14/12/2025
// Updated by Anthony - 21/01/2026

/// <summary>
/// Residential building extends ParentBuilding by adding a quality value.
/// Quality is derived from tier using max_quality.
/// </summary>
public class ResidentialBuilding : ParentBuilding
{
    [SerializeField] protected int[] max_quality;

    protected int quality = 0;

    protected override void Awake()
    {
        base.Awake();

        ApplyQualityFromTier();
    }

    protected override void TierSelection()
    {
        base.TierSelection();

        ApplyQualityFromTier();
    }

    private void ApplyQualityFromTier()
    {
        if (max_quality == null || max_quality.Length <= 0)
        {
            quality = 0;
            return;
        }

        int tier_index = tier - 1;

        if (tier_index < 0)
        {
            tier_index = 0;
        }

        if (tier_index >= max_quality.Length)
        {
            tier_index = max_quality.Length - 1;
        }

        quality = max_quality[tier_index];
    }

    public int GetQuality()
    {
        return quality;
    }
}
