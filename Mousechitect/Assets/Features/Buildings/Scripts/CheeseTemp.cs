using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheeseTemp
{
    protected float prodution_time;
    protected float milk_cost;
    protected int scrap_cost;

    public float GetMilkCost()
    {
        return milk_cost;
    }
    public int GetScrapCost()
    {
        return scrap_cost;
    }

    public float GetProductionTime()
    {
        return prodution_time;
    }

    public CheeseTemp() 
    {
        prodution_time = 0;
        milk_cost = 0;
        scrap_cost = 0;
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
