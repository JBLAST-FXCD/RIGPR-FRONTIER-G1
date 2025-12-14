using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Iain Benner 05/12/2025

/// <summary>
/// class can research node with timer and subtract costs and set node to has been researched
/// </summary>
public class ResearchBuilding : ParentBuilding
{
    //Delete these varibles when script is connect to global cheese and scrap counter
    protected int cheese;
    protected int scrap;

    public ResearchBuilding()
    {
        //Delete these varibles when script is connect to global cheese and scrap counter
        cheese = 0;
        scrap  = 0;
    }

    //The player will call the funtion through the UI 
    protected void ResearchNode(ResearchTreeTemp tree)
    {
        if (tree.CheckIfResearched() == false && tree.GetCheeseCost() >= cheese && tree.GetScrapCost() >= scrap)
        {
            cheese -= tree.GetCheeseCost();
            scrap  -= tree.GetScrapCost();
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
