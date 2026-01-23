using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Morale adapter for ResidentialBuilding.
/// This reads tier/max_quality data from ResidentialBuilding without requiring changes to the building scripts.
/// Keeps morale isolated from building implementation details.
/// </summary>
public class ResidentialMoraleAdapter : MonoBehaviour, IMoraleContributor
{
    private const float DEFAULT_MAX_QUALITY = 20.0f;

    [Header("Housing Bonus")]
    [SerializeField] private float house_count_bonus = 0.02f;

    private ResidentialBuilding residential_building;

    private void Awake()
    {
        residential_building = GetComponent<ResidentialBuilding>();
    }

    public MORALE_CONTRIBUTOR_TYPE GetContributorType()
    {
        return MORALE_CONTRIBUTOR_TYPE.TYPE_HOUSING;
    }

    public float GetContributionScore()
    {
        float quality_normalised = GetQualityNormalised();

        float score = quality_normalised + house_count_bonus;

        return Mathf.Clamp01(score);
    }

    private float GetQualityNormalised()
    {
        if (residential_building == null)
        {
            return 0.0f;
        }

        int tier = GetTierValue(residential_building);
        int[] max_quality = GetMaxQualityArray(residential_building);

        if (max_quality == null || max_quality.Length <= 0)
        {
            return 0.0f;
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

        float quality_value = max_quality[tier_index];
        float max_value = GetArrayMax(max_quality);

        if (max_value <= 0.0f)
        {
            max_value = 20.0f;
        }

        return Mathf.Clamp01(quality_value / max_value);
    }


    private int[] GetMaxQualityArray(ResidentialBuilding building)
    {
        // max_quality is protected, so we can't access it directly.
        // To avoid editing ResidentialBuilding.cs, I read it via reflection.
        // This is temporary: once ResidentialBuilding has GetQuality()/GetMaxQuality(), replace this.
        var field = typeof(ResidentialBuilding).GetField("max_quality", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        if (field == null)
        {
            return null;
        }

        return field.GetValue(building) as int[];
    }

    private int GetTierValue(ResidentialBuilding building)
    {
        var field = typeof(ParentBuilding).GetField(
            "tier",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic
        );

        if (field == null)
        {
            return 1;
        }

        return (int)field.GetValue(building);
    }

    private float GetArrayMax(int[] values)
    {
        int i = 0;
        int max = 0;

        while (i < values.Length)
        {
            if (values[i] > max)
            {
                max = values[i];
            }

            ++i;
        }

        return (float)max;
    }
}
