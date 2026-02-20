using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Updated by Iain Benner 20/02/2026
// jess @ 03/02/2026
// <summary>
// Represents a milk tank that can store milk in the game.
// This is used for both tanks and collectors as functionally their storage system is the same.
// </summary>
public class MilkTank : ParentBuilding, IMilkContainer
{
    [SerializeField] protected int[] max_capacitys;
    protected int current_milk_amount = 0;

    public GameObject CONTAINER_GAME_OBJECT => gameObject;
    public int CURRENT_MILK_AMOUNT {get => current_milk_amount; set => current_milk_amount = value; }
    public int[] MAX_MILK_CAPACITYS { get => max_capacitys; set => max_capacitys = value; }
    public int MAX_MILK_CAPACITY { get => max_capacitys[tier - 1]; set => MAX_MILK_CAPACITY = value; }
    public override BuildingType Building_type => BuildingType.tank;
    public BuildingType BUILDING_TYPE => Building_type;

    public bool is_full => current_milk_amount >= MAX_MILK_CAPACITY;
    public bool is_nearly_full => current_milk_amount >= (MAX_MILK_CAPACITY * 0.8f);

    protected new void Start()
    {
        MilkManager.Instance.RegisterContainer(this);

        ConstructTier();
    }

    private void OnDestroy()
    {
        MilkManager.Instance.UnregisterContainer(this);
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
        if (CURRENT_MILK_AMOUNT + MILK <= MAX_MILK_CAPACITY)
        {
            CURRENT_MILK_AMOUNT += MILK;
        }
        else
            CURRENT_MILK_AMOUNT = MAX_MILK_CAPACITY;
    }
}
