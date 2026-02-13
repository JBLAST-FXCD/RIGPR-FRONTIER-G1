using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// jess @ 03/02/2026
// <summary>
// Represents a milk tank that can store milk in the game.
// This is used for both tanks and collectors as functionally their storage system is the same.
// </summary>
public class MilkTank : ParentBuilding, IMilkContainer
{
    public int max_capacity = 50;
    public int current_milk_amount = 0;

    public GameObject CONTAINER_GAME_OBJECT => gameObject;
    public int CURRENT_MILK_AMOUNT {get => current_milk_amount; set => current_milk_amount = value; }
    public int MAX_MILK_CAPACITY { get => max_capacity; set => max_capacity = value; }
    public bool IS_TANK => true;
    public override BuildingType Building_type => BuildingType.tank;

    public bool is_full => current_milk_amount >= max_capacity;
    public bool is_nearly_full => current_milk_amount >= (max_capacity * 0.8f);

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
            mouse_occupants.Add(other.gameObject.GetComponent<MouseTemp>());
    }
}
