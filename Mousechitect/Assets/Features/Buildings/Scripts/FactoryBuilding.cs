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
    [SerializeField] protected CheeseTemp[,] cheese_amount;
    [SerializeField] protected int[] scrap_costs;

    protected CheeseTemp cheese_type;
    protected int scrap_cost;

    //Delete these varible when script is connect to global variable
    protected int population;

    //For counting the factories to know if there to many as per the GDD
    static protected int count;
    protected int id;

    protected float stored_milk;
    protected bool  produce_cheese;
    protected bool  factory_switch;

    public FactoryBuilding()
    {
        cheese_type = new CheeseTemp();

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
    protected void SelectCheese(int input) 
    {
        cheese_type = cheese_amount[tier - 1, input];
    }
    protected new void TierSelection()
    {
        building_prefab = building_prefabs[tier - 1];
        capacity        = capacitys[tier - 1];
        scrap_cost      = scrap_costs[tier - 1];
    }

    //Delay is hard coded because theres variation in the GDD
    protected void UpdradeFactory()
    {
        ResourceManager resources = ResourceManager.instance;

        if (resources.Scrap >= scrap_cost)
        {
            resources.SpendResources(scrap_cost,0);
            factory_switch = false;
            Invoke(nameof(UpdateTier), 60.0f);
        }
    }

    protected new void UpdateTier()
    {
        tier++;
        if (tier > 0 && tier <= capacitys.Length)
        {
            Destroy(building);
            TierSelection();
            building_prefab.transform.localPosition = new Vector3(0, 0, 0);
            building = Instantiate(building_prefab, gameObject.transform);
            factory_switch = true;
        }
    }

    //Each cheese has production time
    protected void CheeseProduction()
    {
        //Checks if theres enought mise for factory as per GDD. id starts at 0 not 1
        if (id < population / 20)
        {
            if (cheese_type.GetMilkCost() >= stored_milk && produce_cheese == true)
                Invoke(nameof(CreateCheese), cheese_type.GetProductionTime());
        }
        else
            Debug.Log("Not enough mice to operate this factory");
    }

    //This is apart of CheeseProduction() and is called when its invoked.
    protected void CreateCheese()
    {
        ResourceManager resources = ResourceManager.instance;

        //cheese++
        resources.AddResources(0,1);

        stored_milk -= cheese_type.GetMilkCost();
        resources.SpendResources(cheese_type.GetScrapCost(),0);

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
    protected void AddMilk(float milk)
    {
        if (factory_switch == true)
        {
            stored_milk += milk;
            CheeseProduction();
        }
    }
}