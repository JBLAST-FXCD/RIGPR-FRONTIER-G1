using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

// Updated by Iain Benner 20/02/2026
// jess @ 03/02/2026
// <summary>
// script for the collectors that produce milk over time.
// handles milk production and overflow to tanks when full.
// logic for placement constraints still needs to be added.
// </summary>
public class MilkCollector : ParentBuilding, IMilkContainer
{
    [SerializeField] int[] max_capacitys;
    public float production_interval = 10.0f;
    protected int current_milk_amount = 0;
    private float timer = 0f;

    public bool is_next_to_wall = false; // to be implemented with placement logic pending discussion

    public GameObject CONTAINER_GAME_OBJECT => gameObject;
    public int CURRENT_MILK_AMOUNT { get => current_milk_amount; set => current_milk_amount = value; }
    public int[] MAX_MILK_CAPACITYS { get => max_capacitys; set => max_capacitys = value; }
    public int MAX_MILK_CAPACITY { get => max_capacitys[tier - 1]; set => MAX_MILK_CAPACITY = value; }
    public override BuildingType Building_type => BuildingType.collector;
    public BuildingType BUILDING_TYPE => Building_type;

    private void OnDestroy()
    {
        if (MilkManager.Instance != null)
        {
            MilkManager.Instance.UnregisterContainer(this);
        }
    }

    protected new void Start()
    {
        // register with milk manager
        MilkManager.Instance.RegisterContainer(this);

        ConstructTier();
    }

    public void Update()
    {
        timer += Time.deltaTime;
        if (timer >= production_interval)
        {
            ProduceMilk();
            timer = 0f;
        }
    }

    protected new void OnTriggerStay(Collider other)
    {
        if (other != null && other.tag == "MouseTemp" && mouse_occupants.Count < capacity)
        {
            MouseTemp mouse = other.gameObject.GetComponent<MouseTemp>();
            mouse_occupants.Add(mouse);
            mouse.Home = this;
        }
    }

    private void ProduceMilk()
    {
        if (current_milk_amount < MAX_MILK_CAPACITY)
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
        if (current_milk_amount < MAX_MILK_CAPACITY) return "producing milk";
        if (MilkManager.Instance.GetAvailableTank() != null) return "overflowing milk to tank";
        return "Milk Storage Full, Milk Wasting";
    }

    //For checking if milk can be subtracted.
    public bool CanAfford(int MILK)
    {
        if (CURRENT_MILK_AMOUNT >= MILK)
            return true;

        return false;
    }

    public void SubtractMilk(int MILK)
    {
        CURRENT_MILK_AMOUNT -= MILK;
    }

    public int MilkToAdd()
    {
        return MAX_MILK_CAPACITY - CURRENT_MILK_AMOUNT;
    }
    public void AddMilk(int MILK)
    {
        //Can't add milk only gens milk.
    }
}
