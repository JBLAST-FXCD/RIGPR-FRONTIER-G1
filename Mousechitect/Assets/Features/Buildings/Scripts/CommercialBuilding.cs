using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

// Iain Benner 05/12/2025

/// <summary>
/// Popularity Algorithm fits GDD. The algorithm changes randomly each itaration every 5 to 10 mins.
/// cheese is sold in units at rates maching the GDD.
/// </summary>
public class CommercialBuilding : ParentBuilding
{
    //Delete these varible when script is connect to global variable
    protected int population;

    //For selling
    protected CheeseTypes[] keys;
    protected int[] cheese_amounts;

    //Numbers for PopularityAlgorithm()
    protected int cheese_types;
    protected float[] cheese_popularity;
    protected float remaining_percent;
    protected int max_persent;
    protected int mini_percent;
    protected int remaining_cheese;
    protected int index;

    //Delay for recalculating popularity values
    [SerializeField] protected float mini_pop_delay;
    [SerializeField] protected float max_pop_delay;

    [SerializeField] protected float mini_sell_delay;
    [SerializeField] protected float max_sell_delay;

    public float[] Cheese_popularity { get { return cheese_popularity; } }
    public int[] Cheese_amounts { get { return cheese_amounts; } }


    CommercialBuilding() 
    {
        //These number is based off GDD and is hard coded for the algorithm to work. In future [SerializeField] for designers to access easily 
        cheese_types = Enum.GetNames(typeof(CheeseTypes)).Length;

        //Delete these varible when script is connect to global variable
        population = 20;

        cheese_popularity = new float[cheese_types];
        remaining_percent = 100.0f;
        max_persent       = 50;
        mini_percent      = 5;
        remaining_cheese  = 0;
        index             = 0;

        //In seconds
        mini_pop_delay = 300.0f;
        max_pop_delay  = 600.0f;

        mini_sell_delay = 10;
        max_sell_delay  = 20;
    }

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        //These funtions are looped infinitely per GDD
        RecalculatePopularity();
        SellDelay();
    }

    //The maximum range is limited to prevent any element of the array from having a disproportionate chance of being the maximum number,
    //as the random number is capped due to a minimum percentage for each cheese.
    //Every number above the maximum adds one per cent because higher numbers are reduced to the maximum.
    protected void PickPercent(int i, int max_range)
    {
        //Make the random number a factor of mini_percent because the minimum_percent is equivalent to the percentage for the unit
        int random = UnityEngine.Random.Range(mini_percent, max_range);
        int factor = random - (random % mini_percent);

        cheese_popularity[i] = factor;

        //Record these numbers to contune funtionality outside for loop
        remaining_percent -= cheese_popularity[i];
        remaining_cheese = cheese_popularity.Length - 1 - i;
        index++;
    }

    protected void PopularityAlgorithm()
    {
        //For resetting value when looped
        cheese_popularity = new float[cheese_types];
        remaining_percent = 100.0f;
        remaining_cheese = 0;
        index = 0;

        for (int i = 0; i <= cheese_popularity.Length - 1; i++)
        {
            //Runs when remian percent is not too little 
            if (remaining_percent >= mini_percent * remaining_cheese)
            {
                //Stop the element of the array having a disproportionate chance of being the maximum numbe
                int highest = (int)(remaining_percent - (remaining_cheese - 1) * mini_percent);
                int max_range = max_persent < highest ? max_persent : highest;
                max_range = max_range < remaining_percent ? max_range : (int)remaining_percent;

                PickPercent(i, max_range);
            }
            else
                break;
        }

        //If remaining percent is too little remove enough percent from cheese not remaining
        while (remaining_percent < mini_percent * remaining_cheese)
        {
            for (int j = 0; j <= cheese_popularity.Length - 1; j++)
            {
                if (remaining_percent < mini_percent * remaining_cheese && cheese_popularity[j] > mini_percent)
                {
                    cheese_popularity[j] -= mini_percent;
                    remaining_percent += mini_percent;
                }
            }
        }
        //Stops total popularity  being less than 100%
        if (remaining_percent > 0)
        {
            for (int x = index - 1; x > remaining_cheese; x--)
            {
                if (cheese_popularity[x] < max_persent && remaining_percent > 0)
                {
                    cheese_popularity[x] += mini_percent;
                    remaining_percent -= mini_percent;
                }
            }
        }

        //Repeat loop of recalculating popularity values
        RecalculatePopularity();
    }

    //Recalculates Popularity every 5 to 10 mins as per GDD
    protected void RecalculatePopularity()
    {
        float pop_delay = UnityEngine.Random.Range(mini_pop_delay, max_pop_delay);

        Invoke(nameof(PopularityAlgorithm), pop_delay);
    }

    //Sells cheese ever 10 to 20 seconds as per GDD
    protected void SellDelay()
    {
        float sell_delay = UnityEngine.Random.Range(mini_sell_delay, max_sell_delay);

        Invoke(nameof(Sell), sell_delay);
    }

    protected void Sell()
    {
        ResourceManager resources = ResourceManager.instance;

        for (int i = 0; i <= keys.Length - 1; i++)
        {
            int units = population / 10 * (int)cheese_popularity[i] / mini_percent;

            if(resources.CanAfford(keys[i], units) == true)
            {
                //Later replace scrap and cheese with global scrap and cheese counter
                resources.SpendResources(keys[i], units);
                resources.AddResources(Cheese.GetCheese(keys[i]).scrap_price * units);
            }

            //Repeat loop of selling cheese
            SellDelay();
        }
    }
}