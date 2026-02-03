using System;
using System.Collections.Generic;
using UnityEngine;

// Iain Benner 02/02/2026

/// <summary>
/// Holds all the information for cheeses.
/// </summary>
public enum CheeseTypes
{
    AmericanCheese, Cheddar, Mozzarella, Brie, Gouda, Parmesan, BlueCheese
}

public struct CheeseValues
{
    public float prodution_time;
    public int milk_cost;
    public int scrap_cost;
    public int scrap_price;
}

public class Cheese : MonoBehaviour
{
    //In seconds
    [SerializeField] protected float[] prodution_times;
    [SerializeField] protected int[] milk_costs;
    [SerializeField] protected int[] scrap_costs;
    [SerializeField] protected int[] scrap_price;

    protected static Dictionary<CheeseTypes, CheeseValues> cheeses;

    public Cheese()
    {
        cheeses = new Dictionary<CheeseTypes, CheeseValues>();
    }

    public static CheeseValues GetCheese(CheeseTypes key)
    {
        return cheeses[key];
    }

    protected CheeseValues GetCheeseValues(int index)
    {
        CheeseValues rv = new CheeseValues();

        rv.prodution_time = prodution_times[index];
        rv.milk_cost = milk_costs[index];
        rv.scrap_cost = scrap_costs[index];
        rv.scrap_cost = scrap_price[index];

        return rv;
    }

    public void Start()
    {
        foreach (CheeseTypes c in Enum.GetValues(typeof(CheeseTypes)))
            cheeses.Add(c, GetCheeseValues((int)c));
    }
}
