using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// jess @ 03/02/2026
// <summary>
// script for the collectors that produce milk over time.
// handles milk production and overflow to tanks when full.
// logic for placement constraints still needs to be added.
// </summary>
public class MilkCollector : ParentBuilding, IMilkContainer
{
    public float production_interval = 10.0f;
    public int max_milk_capacity = 20;
    public int current_milk_amount = 0;
    private float timer = 0f;

    public bool is_next_to_wall = false; // to be implemented with placement logic pending discussion

    public GameObject CONTAINER_GAME_OBJECT => gameObject;
    public int CURRENT_MILK_AMOUNT { get => current_milk_amount; set => current_milk_amount = value; }
    public int MAX_MILK_CAPACITY { get => max_milk_capacity; set => max_milk_capacity = value; }
    public bool IS_TANK => false;

    protected new void Start()
    {
        // register with milk manager
        MilkManager.Instance.RegisterContainer(this);

        ConstructTier();
    }

    protected new void Update()
    {
        timer += Time.deltaTime;
        if (timer >= production_interval)
        {
            ProduceMilk();
            timer = 0f;
        }
    }

    private void ProduceMilk()
    {
        if (current_milk_amount < max_milk_capacity)
        {
            current_milk_amount++;
        }
        else
        {
            AttemptOverflow();
        }
    }

    private void AttemptOverflow()
    {
        MilkTank available_tank = MilkManager.Instance.GetAvailableTank();

        if (available_tank != null)
        {
            available_tank.CURRENT_MILK_AMOUNT++;
        }
        else
        {
            // No available tank found milk is wasted
        }
    }

    public string GetStatus()
    {
        // mainly for debugging purposes, returns current status of milk collector
        if (current_milk_amount < max_milk_capacity) return "producing milk";
        if (MilkManager.Instance.GetAvailableTank() != null) return "overflowing milk to tank";
        return "Milk Storage Full, Milk Wasting";
    }
}
