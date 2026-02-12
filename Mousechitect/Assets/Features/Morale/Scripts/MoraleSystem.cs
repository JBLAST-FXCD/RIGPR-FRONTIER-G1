using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;

//Anthony - 21/01/26

/// <summary>
/// City-wide morale system.
/// Morale is calculated as a weighted blend of multiple factors (housing, food, recreation, aesthetics).
/// Each factor is contributed by scene objects implementing IMoraleContributor.
/// </summary>
public class MoraleSystem : MonoBehaviour
{
    public static MoraleSystem Instance { get; private set; }

    private const float DEFAULT_GLOBAL_MORALE = 0.14f;
    private const float DEFAULT_FOOD_SCORE = 0.50f;
    private const float DEFAULT_RECREATION_SCORE = 0.20f;
    private const float DEFAULT_AESTHETICS_SCORE = 0.00f;

    private const float HOUSING_WEIGHT = 0.40f;
    private const float FOOD_WEIGHT = 0.20f;
    private const float RECREATION_WEIGHT = 0.20f;
    private const float AESTHETICS_WEIGHT = 0.20f;

    private const float MORALE_UPDATE_INTERVAL = 1.0f;

    [Header("Morale Output")]
    [SerializeField] private float global_morale = DEFAULT_GLOBAL_MORALE;
    [SerializeField] private float morale_score = 0.0f;

    [Header("Morale Smoothing")]
    [SerializeField] private float morale_smoothing = 0.15f;

    [Header("Gameplay Modifiers")]
    [SerializeField] private float productivity_modifier = 1.0f;
    [SerializeField] private float retention_modifier = 1.0f;
    [SerializeField] private float arrival_chance_modifier = 1.0f;

    [Header("Debug")]
    [SerializeField]
    public bool is_debug_override = false;

    // Anthony - 10/2/2026
    [SerializeField] private float baseline_morale = DEFAULT_GLOBAL_MORALE;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public void SetMoraleScore(float value) => morale_score = value;

    public void SetProductivityModifier(float value) => productivity_modifier = value;

    public void SetRetentionModifier(float value) => retention_modifier = value;

    public void SetArrivalChanceModifier(float value) => arrival_chance_modifier = value;


    public float GetGlobalMorale()
    {
        return global_morale;
    }

    public float GetMoraleScore()
    {
        return morale_score;
    }

    public float GetProductivityModifier()
    {
        return productivity_modifier;
    }

    public float GetRetentionModifier()
    {
        return retention_modifier;
    }

    public float GetArrivalChanceModifier()
    {
        return arrival_chance_modifier;
    }

    public float GetBaselineMorale()
    {
        return baseline_morale;
    }

    private void Start()
    {
        StartCoroutine(MoraleUpdateLoop());
    }

    private IEnumerator MoraleUpdateLoop()
    {
        while (true)
        {
            if (!is_debug_override)
            {
                bool has_housing;
                bool has_food;
                bool has_recreation;
                bool has_aesthetics;

                float housing_score = CollectAverageContribution(MORALE_CONTRIBUTOR_TYPE.TYPE_HOUSING, out has_housing);
                float food_score = CollectAverageContribution(MORALE_CONTRIBUTOR_TYPE.TYPE_FOOD, out has_food);
                float recreation_score = CollectAverageContribution(MORALE_CONTRIBUTOR_TYPE.TYPE_RECREATION, out has_recreation);
                float aesthetics_score = CollectAverageContribution(MORALE_CONTRIBUTOR_TYPE.TYPE_AESTHETICS, out has_aesthetics);

                if (!has_food) food_score = DEFAULT_FOOD_SCORE;
                if (!has_recreation) recreation_score = DEFAULT_RECREATION_SCORE;
                if (!has_aesthetics) aesthetics_score = DEFAULT_AESTHETICS_SCORE;

                /*
                            if (!has_food) food_score = DEFAULT_FOOD_SCORE;
                            if (!has_recreation) recreation_score = DEFAULT_RECREATION_SCORE;
                            if (!has_aesthetics) aesthetics_score = DEFAULT_AESTHETICS_SCORE;
                */

                float target_morale =
                    (housing_score * HOUSING_WEIGHT) +
                    (food_score * FOOD_WEIGHT) +
                    (recreation_score * RECREATION_WEIGHT) +
                    (aesthetics_score * AESTHETICS_WEIGHT);

                baseline_morale = Mathf.Clamp01(target_morale);

                // Update each mouse morale toward the baseline + preference delta
                MouseTemp[] mice = FindObjectsOfType<MouseTemp>();

                float avg_mouse_morale = baseline_morale;

                if (mice.Length > 0)
                {
                    float sum = 0.0f;

                    for (int i = 0; i < mice.Length; i++)
                    {
                        if (mice[i] == null) continue;

                        mice[i].UpdatePerMouseMorale(baseline_morale);
                        sum += mice[i].MouseMorale;
                    }

                    avg_mouse_morale = sum / mice.Length;
                }

                // Global morale now becomes average mouse morale (GDD behaviour)
                global_morale = Mathf.Lerp(global_morale, avg_mouse_morale, morale_smoothing);
                global_morale = Mathf.Clamp01(global_morale);

                morale_score = (global_morale * 2.0f) - 1.0f;

                UpdateGameplayModifiers(morale_score);


                yield return new WaitForSeconds(MORALE_UPDATE_INTERVAL);
            }
            UpdateGameplayModifiers(morale_score);
            yield return new WaitForSeconds(MORALE_UPDATE_INTERVAL);

        }
    }

    private float CollectAverageContribution(MORALE_CONTRIBUTOR_TYPE contributor_type, out bool found_any)
    {
        MonoBehaviour[] behaviours = FindObjectsOfType<MonoBehaviour>();

        float sum = 0.0f;
        int count = 0;

        int i = 0;

        while (i < behaviours.Length)
        {
            IMoraleContributor contributor = behaviours[i] as IMoraleContributor;

            if (contributor != null && contributor.GetContributorType() == contributor_type)
            {
                sum += contributor.GetContributionScore();
                count++;
            }

            ++i;
        }

        found_any = (count > 0);

        if (count <= 0)
        {
            return 0.0f;
        }

        return Mathf.Clamp01(sum / count);
    }


    private void UpdateGameplayModifiers(float score)
    {
        // Score is -1 - 1.
        // Neutral: 1.0 modifiers.
        // Negative: reduce modifiers.
        // Positive: increase modifiers slightly.

        if (score >= 0.0f)
        {
            productivity_modifier = Mathf.Lerp(1.0f, 1.25f, score);
            retention_modifier = Mathf.Lerp(1.0f, 1.20f, score);
            arrival_chance_modifier = Mathf.Lerp(1.0f, 1.15f, score);
        }
        else
        {
            float t = Mathf.Abs(score);

            productivity_modifier = Mathf.Lerp(1.0f, 0.75f, t);
            retention_modifier = Mathf.Lerp(1.0f, 0.70f, t);
            arrival_chance_modifier = Mathf.Lerp(1.0f, 0.80f, t);
        }
    }
}

