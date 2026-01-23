using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MORALE_CONTRIBUTOR_TYPE
{
    TYPE_HOUSING,
    TYPE_FOOD,
    TYPE_RECREATION,
    TYPE_AESTHETICS
}

/// <summary>
/// Simple interface for anything that can contribute to the city morale.
/// Contributors return a normalised score (0 - 1) for their type.
/// </summary>
public interface IMoraleContributor
{
    MORALE_CONTRIBUTOR_TYPE GetContributorType();

    float GetContributionScore();
}

