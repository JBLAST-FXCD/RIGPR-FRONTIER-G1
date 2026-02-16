using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anthony - 02/02/2026
// Morale contributor that scores "Food Variety" based on ACTIVE cheese production variety.
// ACTIVE variety = number of distinct cheese types currently selected across all factories.
public class FoodVarietyMoraleAdapter : MonoBehaviour, IMoraleContributor
{
    [SerializeField] private int types_for_max_score = 7; // total CheeseTypes

    public MORALE_CONTRIBUTOR_TYPE GetContributorType()
    {
        return MORALE_CONTRIBUTOR_TYPE.TYPE_FOOD;
    }

    public float GetContributionScore()
    {
        ResourceManager rm = ResourceManager.instance;
        if (rm == null) return 0.0f;

        // Uses ACTIVE variety, not "cheese produced historically".
        int variety = rm.GetActiveCheeseVarietyCount();

        // Map 0..types_for_max_score -> 0..1
        float score = Mathf.Clamp01((float)variety / types_for_max_score);

        //Debug.Log($"[Morale-Food] ACTIVE variety={variety}/{types_for_max_score} score={score:0.00}");
        return score;
    }
}


