using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Iain Benner 05/12/2025

/// <summary>
/// class can research node with timer and subtract costs and set node to has been researched
/// </summary>
public class ResearchBuilding : ParentBuilding
{
    //The player will call the funtion through the UI 
    protected void ResearchNode(ResearchTreeTemp tree)
    {
        ResourceManager resources = ResourceManager.instance;

        if (tree.CheckIfResearched() == false && tree.GetCheeseCost() >= resources.Cheese && tree.GetScrapCost() >= resources.Scrap)
        {
            resources.SpendResources(tree.GetScrapCost(), tree.GetCheeseCost());
            StartCoroutine(CompleteResearch(tree));
        }
    }

    protected IEnumerator CompleteResearch(ResearchTreeTemp tree)
    {
        yield return new WaitForSeconds(tree.GetProductionTime());

        //Add information thats need updating on research completion before IsResearched().
        tree.IsResearched();
    }
}
