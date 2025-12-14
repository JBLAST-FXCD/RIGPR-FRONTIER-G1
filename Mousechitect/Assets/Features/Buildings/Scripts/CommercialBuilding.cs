using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Iain Benner 05/12/2025

/// <summary>
/// Popularity Algorithm fits GDD. The algorithm changes randomly each itaration every 5 to 10 mins.
/// cheese is sold in units at rates maching the GDD.
/// </summary>
public class CommercialBuilding : ParentBuilding
{
    protected int[] cheese_prices;
    protected int cheese_amount;

    //Delete these varibles when script is connect to global cheese and scrap counter
    protected int cheese;
    protected int scrap;

    //Numbers for PopularityAlgorithm()
    protected float[] cheese_popularity;
    protected float remaining_percent;
    protected int mini_percent;
    protected int remaining_cheese;
    protected int index;

    //Delay for recalculating popularity values
    [SerializeField] protected float mini_pop_delay;
    [SerializeField] protected float max_pop_delay;
    protected float pop_delay;

    [SerializeField] protected float mini_sell_delay;
    [SerializeField] protected float max_sell_delay;
    protected float sell_delay;
    CommercialBuilding() 
    {
        //These number is based off GDD and is hard coded for the algorithm to work. In future [SerializeField] for designers to access easily 
        cheese_prices = new int[7] {10, 15, 25, 40, 60, 85, 100 };
        cheese_amount = 7;

        //Delete these varibles when script is connect to global cheese and scrap counter
        cheese = 0;
        scrap = 0;

        cheese_popularity = new float[cheese_amount];
        remaining_percent = 100.0f;
        mini_percent      = 5;
        remaining_cheese  = 0;
        index             = 0;

        //In seconds
        mini_pop_delay = 300.0f;
        max_pop_delay  = 600.0f;
        pop_delay      = 0;

        mini_sell_delay = 10;
        max_sell_delay  = 20;
        sell_delay      = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Random.InitState((int)DateTime.Now.Ticks);

        //These funtions are looped infinitely per GDD
        RecalculatePopularity();
        SellDelay();
    }

    protected new void Update()
    {
        //For debuging the popularity numbers
        if (Input.GetKeyDown(KeyCode.C))
        {
            cheese_popularity = new float[cheese_amount];
            float temp = 0;
            PopularityAlgorithm();
            for (int i = 0; i < cheese_popularity.Length; i++)
                Debug.Log(cheese_popularity[i]);
            for (int j = 0; j < cheese_popularity.Length; j++)
                temp += cheese_popularity[j];
            Debug.Log(temp);
        }
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

        //Record these number to contune funtionality outside for loop
        remaining_percent -= cheese_popularity[i];
        remaining_cheese = cheese_popularity.Length - 1 - i;
        index++;
    }

    protected void PopularityAlgorithm()
    {
        for (int i = 0; i <= cheese_popularity.Length - 1; i++)
        {
            //Stop the element of the array having a disproportionate chance of being the maximum numbe
            if (remaining_percent > 100 - remaining_cheese * mini_percent && remaining_percent > mini_percent * remaining_cheese)
            {
                PickPercent(i, (int)(remaining_percent - (remaining_cheese - 1) * mini_percent));
            }
            //Runs when remian percent is not too little 
            else if (remaining_percent >= mini_percent * remaining_cheese)
            {
                PickPercent(i, (int)remaining_percent);
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

        //Remaing cheese equal mini_percentmum percent
        for (int k = index; k <= cheese_popularity.Length - 1; k++)
        {
            cheese_popularity[k] += mini_percent;
        }

        //Repeat loop of recalculating popularity values
        RecalculatePopularity();
    }

    //Recalculates Popularity every 5 to 10 mins as per GDD
    protected void RecalculatePopularity()
    {
        pop_delay = UnityEngine.Random.Range(mini_pop_delay, max_pop_delay);

        cheese_popularity = new float[cheese_amount];
        Invoke(nameof(PopularityAlgorithm), pop_delay);
    }

    //Sells cheese ever 10 to 20 seconds as per GDD
    protected void SellDelay()
    {
        sell_delay = UnityEngine.Random.Range(mini_sell_delay, max_sell_delay);

        Invoke(nameof(Sell), pop_delay);
    }

    protected void Sell()
    {
        for (int i = 0; i <= cheese_prices.Length - 1; i++)
        {
            int units = (int)cheese_popularity[i] / mini_percent;

            if(cheese >= units)
            {
                //Later replace scrap and cheese with global scrap and cheese counter
                cheese -= units;
                scrap += cheese_prices[i] * units;
            }

            //Repeat loop of selling cheese
            SellDelay();
        }
    }
}