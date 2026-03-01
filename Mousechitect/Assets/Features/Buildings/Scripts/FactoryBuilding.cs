using System;
using System.Collections;
using UnityEngine;

// Iain Benner 05/12/2025

/// <summary>
/// select cheese funtion is ready for player input to select cheese.
/// the scrap cost for building changes depending on tier and upgrading building has delay.
/// cheese is produced with delay and milk cost dependent on type of cheese.
/// cheese is made if theres enough milk or player input fitting GDD.
/// cheese is not produced if there are too many factories for the population, as per GDD.
/// </summary>
public class FactoryBuilding : ParentBuilding, IMilkContainer
{
    [SerializeField] protected int[] milk_capasitys;

    // Anthony - Tier based cheese selection
    private CheeseTypes produced_cheese_type = CheeseTypes.AmericanCheese;
    private CheeseTypes selected_cheese = CheeseTypes.AmericanCheese;

    // runtime allowed list for this tier
    private CheeseTypes[] allowed_cheese_types;

    // per-tier sets (tier 1-3)
    private static readonly CheeseTypes[][] cheese_sets_by_tier =
    {
        new[] { CheeseTypes.AmericanCheese, CheeseTypes.Cheddar, CheeseTypes.Mozzarella }, // Tier 1
        new[] { CheeseTypes.Brie, CheeseTypes.Gouda },               // Tier 2
        new[] { CheeseTypes.Parmesan, CheeseTypes.BlueCheese }       // Tier 3
    };

    protected CheeseTypes cheese_type;

    //Delete these varible when script is connect to global variable
    protected int population;

    //For counting the factories to know if there to many as per the GDD
    static protected int count;
    protected int id;

    protected float build_time;
    protected int milk_capasity;
    protected int stored_milk;
    protected bool  produce_cheese;
    protected bool  factory_switch;

    public bool IsActive { get { return factory_switch; } }
    public bool Produce_cheese { get { return produce_cheese; } set { produce_cheese = value; } }
    public CheeseTypes Cheese_type { get { return cheese_type; } }
    public override BuildingType Building_type => BuildingType.factory;

    //Milk interface
    public GameObject CONTAINER_GAME_OBJECT => gameObject;
    public int CURRENT_MILK_AMOUNT { get => stored_milk; set => stored_milk = value; }
    public int[] MAX_MILK_CAPACITYS { get => milk_capasitys; set => milk_capasitys = value; }
    public int MAX_MILK_CAPACITY { get => milk_capasity; set => milk_capasity = value; }

    public FactoryBuilding()
    {
        //Default
        cheese_type = CheeseTypes.AmericanCheese;

        //Delete these varible when script is connect to global variable
        population = 20;

        id = count;

        build_time = 60.0f;
        stored_milk = 0;
        produce_cheese = false;
        factory_switch = true;
    }

    protected override void Start()
    {
        scrap_cost = scrap_costs[0];

        if (resources.CanAfford(scrap_cost))
        {
            //Warns the player if cheese can't be produced when the building is constructed
            if (id >= population / 20)
                Debug.Log("Not enough mice to operate this factory");

            if (ResourceManager.instance != null)
                ResourceManager.instance.RegisterOrUpdateFactoryCheeseType(this, produced_cheese_type);

            count++;
            MilkManager.Instance.RegisterContainer(this);
            ConstructTier();
            RefreshAllowedCheesesForTier();
        }
        else
            Destroy(this);
    }

    private void OnDestroy()
    {
        MilkManager.Instance.UnregisterContainer(this);
    }

    //for player to select cheese
    public void SelectCheese(CheeseTypes input) 
    {
        cheese_type = input;
    }

    protected override void TierSelection()
    {
        building_prefab = building_prefabs[tier - 1];
        capacity        = capacitys[tier - 1];
        scrap_cost      = scrap_costs[tier - 1];
        milk_capasity   = milk_capasitys[tier - 1];
    }

    public override void UpdradeBuilding()
    {
        if (tier + 1 <= capacitys.Length && !upgrading)
        {
            //Updated scrap cost.
            scrap_cost = scrap_costs[tier - 1];

            if (resources.CanAfford(scrap_cost))
            {
                resources.SpendResources(scrap_cost);
                factory_switch = false;
                upgrading = true;
                Invoke(nameof(UpdateTier), build_time);
            }
        }
    }

    protected override void UpdateTier()
    {
        Destroy(building);
        tier++;
        TierSelection();
        building_prefab.transform.localPosition = new Vector3(0, 0, 0);
        building = Instantiate(building_prefab, gameObject.transform);
        this.GetComponent<BoxCollider>().center = building.transform.Find("EntrancePoint").localPosition;
        factory_switch = true;
        upgrading = false;
        RefreshAllowedCheesesForTier();
    }

    //Each cheese has production time
    protected IEnumerator CheeseProduction()
    {
        if (produce_cheese)
        {
            CheeseValues cheese = Cheese.GetCheese(cheese_type);

            yield return new WaitForSeconds(cheese.prodution_time);

            //Checks if theres enought mise for factory as per GDD. id starts at 0 not 1
            if (id < population / 20)
            {
                if (CanAfford(cheese.milk_cost) && resources.CanAfford(scrap_cost))
                    CreateCheese();
                else
                    StartCoroutine(CheeseProduction());
            }
            else
                Debug.Log("Not enough mice to operate this factory");
        }
    }

    //This is apart of CheeseProduction() and is called when its invoked.
    protected void CreateCheese()
    {
        CheeseValues cheese = Cheese.GetCheese(cheese_type);

        SubtractMilk(cheese.milk_cost);
        resources.SpendResources(cheese.scrap_cost);
        resources.AddResources(cheese_type, 1);
        StartCoroutine(CheeseProduction());
    }

    //For player to create cheese when factory is running
    public void ProduceCheeseSwitch()
    {
        if (produce_cheese)
            produce_cheese = false;
        else
            produce_cheese = true;

        StartCoroutine(CheeseProduction());
    }

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
    //Fits GDD requirement of making cheese when theres enough milk
    public bool AddMilk(int MILK)
    {
        if (factory_switch == true && CURRENT_MILK_AMOUNT + MILK <= MAX_MILK_CAPACITY)
        {
            CURRENT_MILK_AMOUNT += MILK;
            CheeseProduction();
            return true;
        }
        return false;
    }

    // Created by Anthony - 08/02/2026
    private void RefreshAllowedCheesesForTier()
    {
        int tier_index = Mathf.Clamp(tier - 1, 0, cheese_sets_by_tier.Length - 1);
        allowed_cheese_types = cheese_sets_by_tier[tier_index];

        // Clamp selection if it's not valid for this tier
        bool valid = false;
        for (int i = 0; i < allowed_cheese_types.Length; i++)
        {
            if (allowed_cheese_types[i] == selected_cheese) { valid = true; break; }
        }

        if (!valid)
            selected_cheese = allowed_cheese_types[0];

        // Sync internal recipe values with selected cheese
        SelectCheese(selected_cheese);

        // If you're using the active variety registry, keep it updated
        if (ResourceManager.instance != null)
            ResourceManager.instance.RegisterOrUpdateFactoryCheeseType(this, selected_cheese);
    }
    
    // Updates by Anthony - 05/02/2026
    // Cycles this factory's currently selected cheese type (per-factory, not global).
    // Also syncs the internal recipe data and informs ResourceManager so ACTIVE variety updates correctly.
    public void CycleCheeseType()
    {
        if (allowed_cheese_types == null || allowed_cheese_types.Length == 0)
            RefreshAllowedCheesesForTier();

        // Find current index
        int idx = 0;
        for (int i = 0; i < allowed_cheese_types.Length; i++)
            if (allowed_cheese_types[i] == selected_cheese) { idx = i; break; }

        CheeseTypes before = selected_cheese;
        selected_cheese = allowed_cheese_types[(idx + 1) % allowed_cheese_types.Length];

        SelectCheese(selected_cheese);

        Debug.Log($"[Factory] {name} switched {before} -> {selected_cheese}");

        if (ResourceManager.instance != null)
            ResourceManager.instance.RegisterOrUpdateFactoryCheeseType(this, selected_cheese);
    }

    private void OnDisable()
    {
        // If the factory is removed/destroyed, remove it from active variety tracking.
        if (ResourceManager.instance != null)
            ResourceManager.instance.UnregisterFactory(this);
    }

    public CheeseTypes[] GetAvalibleCheese()
    {
        return allowed_cheese_types;
    }

    public CheeseTypes GetSelectedCheese()
    {
        return selected_cheese;
    }
}