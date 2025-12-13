using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ResearchTreeTemp : MonoBehaviour
{
    protected int cheese_cost;
    protected int scrap_cost;

    protected float prodution_time;

    protected bool is_researched;

    public int GetCheeseCost()
    {
        return cheese_cost;
    }
    public int GetScrapCost()
    {
        return scrap_cost;
    }
    public float GetProductionTime()
    {
        return prodution_time;
    }
    public bool CheckIfResearched()
    {
        return is_researched;
    }

    public void IsResearched()
    {
        is_researched = true;
    }

    public ResearchTreeTemp()
    {
        cheese_cost = 0;
        scrap_cost = 0;
        prodution_time = 5f;
        is_researched = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
