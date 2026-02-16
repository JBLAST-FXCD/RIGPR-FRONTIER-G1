using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Iain Benner 05/12/2025

/// <summary>
/// class can research node with timer and subtract costs and set node to has been researched
/// </summary>
public class ResearchBuilding : ParentBuilding
{
    public override BuildingType Building_type => BuildingType.research;

    //The player will call the funtion through the UI 
    protected void ResearchNode(ResearchTreeTemp tree)
    {
        ResourceManager resources = ResourceManager.instance;

        if (tree.Is_researched == false && resources.CanAfford(tree.Scrap_cost, tree.Type, tree.Cheese_amount) == true)
        {
            resources.SpendResources(tree.Scrap_cost, tree.Type, tree.Cheese_amount);
            StartCoroutine(CompleteResearch(tree));
        }
    }

    protected IEnumerator CompleteResearch(ResearchTreeTemp tree)
    {
        yield return new WaitForSeconds(tree.Production_time);

        //Add information thats need updating on research completion before IsResearched().
        tree.IsResearched();
    }
}
