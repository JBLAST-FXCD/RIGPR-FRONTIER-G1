using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anthony - 2/2/2026

public class FoodVarietyMoraleAdapter : MonoBehaviour, IMoraleContributor
{
    [SerializeField] private int types_for_max_score = 3; // simple factory era

    public MORALE_CONTRIBUTOR_TYPE GetContributorType()
    {
        return MORALE_CONTRIBUTOR_TYPE.TYPE_FOOD;
    }

    public float GetContributionScore()
    {
        ResourceManager rm = ResourceManager.instance;
        if (rm == null) return 0.0f;

        int variety = ResourceManager.instance.GetActiveCheeseVarietyCount();
        float score = Mathf.Clamp01((float)variety / types_for_max_score);

        Debug.Log($"[Morale-Food] variety={variety}/{types_for_max_score} score={score:0.00}");
        return score;
    }
}

