using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Iain Benner 05/12/2025

/// <summary>
/// select cheese funtion is ready for player input to select cheese.
/// the scrap cost for building changes depending on tier and upgrading building has delay.
/// cheese is produced with delay and milk cost dependent on type of cheese.
/// cheese is made if theres enough milk or player input fitting GDD.
/// cheese is not produced if there are too many factories for the population, as per GDD.
/// </summary>
public class FactoryBuilding : ParentBuilding
{
    //first element is for rarity and second element is for cheese type.
    [SerializeField] protected int[] scrap_costs;
    [SerializeField] protected int[] milk_capasitys;
    [SerializeField] private CheeseTypes produced_cheese_type = CheeseTypes.AmericanCheese;

    [SerializeField]
    private CheeseTypes[] allowed_cheese_types =
    {
        CheeseTypes.AmericanCheese,
        CheeseTypes.Cheddar,
        CheeseTypes.Mozzarella
    };


    protected CheeseValues cheese_type;
    protected int scrap_cost;

    //Delete these varible when script is connect to global variable
    protected int population;

    //For counting the factories to know if there to many as per the GDD
    static protected int count;
    protected int id;

    protected int milk_capasity;
    protected int stored_milk;
    protected bool  produce_cheese;
    protected bool  factory_switch;

    public bool IsActive { get { return factory_switch; } }
    public int Stored_milk { get { return stored_milk; } }
    public int Milk_capacity { get { return milk_capasity; } }

    public FactoryBuilding()
    {
        cheese_type = new CheeseValues();

        //Delete these varible when script is connect to global variable
        population = 20;

        id = count;

        stored_milk = 0;
        produce_cheese = false;
        factory_switch = true;
    }

    void Start()
    {
        //Warns the player if cheese can't be produced when the building is constructed
        if (id >= population / 20)
            Debug.Log("Not enough mice to operate this factory");

        if (ResourceManager.instance != null)
            ResourceManager.instance.RegisterOrUpdateFactoryCheeseType(this, produced_cheese_type);

    count++;
        ConstructTier();
    }

    protected override void Update()
    {
        //This is for debuging.
        //Mise will leave when certain conditions are met, depending on the building type.
        if (Input.GetKeyDown(KeyCode.E) && mouse_occupants.Count > 0)
        {
            MouseLeave(mouse_occupants[0]);
        }

        //This is for debuging.
        //Buildings will be upgraded when certain conditions are met, depending on the building type.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UpdateTier();
        }

        //This is for debuging.
        //Later this will be a bottun in the UI.
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (produce_cheese)
                produce_cheese = false;
            else
                produce_cheese = true;

            ProduceCheese(produce_cheese);
        }
    }

    //for player to select cheese
    protected void SelectCheese(CheeseTypes input) 
    {
        cheese_type = Cheese.GetCheese(input);
    }

    protected override void TierSelection()
    {
        building_prefab = building_prefabs[tier - 1];
        capacity        = capacitys[tier - 1];
        scrap_cost      = scrap_costs[tier - 1];
        milk_capasity   = milk_capasitys[tier - 1];
    }

    //Delay is hard coded because theres variation in the GDD
    protected void UpdradeFactory()
    {
        ResourceManager resources = ResourceManager.instance;

        if (resources.CanAfford(scrap_cost) == true)
        {
            resources.SpendResources(scrap_cost);
            factory_switch = false;
            Invoke(nameof(UpdateTier), 60.0f);
        }
    }

    protected override void UpdateTier()
    {
        tier++;
        if (tier > 0 && tier <= capacitys.Length)
        {
            Destroy(building);
            TierSelection();
            building_prefab.transform.localPosition = new Vector3(0, 0, 0);
            building = Instantiate(building_prefab, gameObject.transform);
            factory_switch = true;
            this.GetComponent<BoxCollider>().center = building.transform.Find("EntrancePoint").localPosition;
        }
    }

    //Each cheese has production time
    protected void CheeseProduction()
    {
        //Checks if theres enought mise for factory as per GDD. id starts at 0 not 1
        if (id < population / 20)
        {
            if (cheese_type.milk_cost >= stored_milk && produce_cheese == true)
                Invoke(nameof(CreateCheese), cheese_type.prodution_time);
        }
        else
            Debug.Log("Not enough mice to operate this factory");
    }

    //This is apart of CheeseProduction() and is called when its invoked.
    protected void CreateCheese()
    {
        ResourceManager resources = ResourceManager.instance;

        ResourceManager.instance.AddResources(produced_cheese_type, 1);

        stored_milk -= cheese_type.milk_cost;
        resources.SpendResources(cheese_type.scrap_cost);

        //Repeat cheese prodution until milk runs out or player switches produce_cheese to false
        CheeseProduction();
    }

    //For player to create cheese when factory is running
    protected void ProduceCheese(bool input)
    {
        if (factory_switch == true)
        {
            produce_cheese = input;
            CheeseProduction();
        }
    }

    //Fits GDD requirement of making cheese when theres enough milk
    protected void AddMilk(int milk)
    {
        if (factory_switch == true && stored_milk + milk <= milk_capasity)
        {
            stored_milk += milk;
            CheeseProduction();
        }
    }

    // Updates by Anthony - 05/02/2026
    // Cycles this factory's currently selected cheese type (per-factory, not global).
    // Also syncs the internal recipe data and informs ResourceManager so ACTIVE variety updates correctly.
    public void CycleCheeseType()
    {
        // Safety: no allowed types means there's nothing to cycle.
        if (allowed_cheese_types == null || allowed_cheese_types.Length == 0)
            return;

        CheeseTypes before = produced_cheese_type;

        // Find current type index in the allowed list.
        int idx = 0;
        for (int i = 0; i < allowed_cheese_types.Length; i++)
        {
            if (allowed_cheese_types[i] == produced_cheese_type)
            {
                idx = i;
                break;
            }
        }

        // Wrap around to the start once we hit the end.
        produced_cheese_type = allowed_cheese_types[(idx + 1) % allowed_cheese_types.Length];

        // Keep the internal cheese recipe selection in sync with the enum.
        SelectCheese(produced_cheese_type);

        Debug.Log($"[Factory] {name} switched {before} -> {produced_cheese_type}");

        // Update active cheese variety tracking (used by morale food variety).
        if (ResourceManager.instance != null)
            ResourceManager.instance.RegisterOrUpdateFactoryCheeseType(this, produced_cheese_type);
    }

    private void OnDisable()
    {
        // If the factory is removed/destroyed, remove it from active variety tracking.
        if (ResourceManager.instance != null)
            ResourceManager.instance.UnregisterFactory(this);
    }


}