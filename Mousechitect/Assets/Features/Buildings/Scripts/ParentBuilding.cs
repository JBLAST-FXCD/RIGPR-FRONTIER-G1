using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Iain Benner 05/12/2025

/// <summary>
/// Designer can set up mesh and data for tiers of buildings.
/// On start the correct varition of the building is picked.
/// Mice can enter and leave the building.
/// </summary>

public enum BuildingType
{
    residental, factory, market, research, tank, collector
}

public class ParentBuilding : MonoBehaviour
{
    //Varibles for designers to create tiers.
    [SerializeField] protected int tier;
    [SerializeField] protected GameObject[] building_prefabs;
    [SerializeField] protected int[] capacitys;
    [SerializeField] protected int[] scrap_costs;

    //Varibles to select the correct paramiters for the tier.
    protected GameObject building_prefab;
    protected GameObject building;
    protected List<MouseTemp> mouse_occupants;
    protected int capacity;
    protected int scrap_cost;
    protected bool upgrading;

    protected ResourceManager resources = ResourceManager.instance;

    public int Tier { get { return tier; } }
    public GameObject Building { get { return building; } }
    public List<MouseTemp> Mouse_occupants { get { return mouse_occupants; } }
    public virtual BuildingType Building_type { get; }

    public ParentBuilding()
    {
        building_prefab = null;
        mouse_occupants = new List<MouseTemp>();

        upgrading = false;
    }

    protected virtual void Start()
    {
        scrap_cost = scrap_costs[0];

        if (resources.CanAfford(scrap_cost))
            ConstructTier();
        else
            Destroy(this);
    }

    protected virtual void TierSelection()
    {
        building_prefab = building_prefabs[tier - 1];
        capacity = capacitys[tier - 1];
        scrap_cost = scrap_costs[tier - 1];
    }

    //The function allows for diffrent varition depending on the designers choise and can be used for when the player upgrades the building.
    protected void ConstructTier()
    {
        if (tier > 0 && tier <= capacitys.Length)
        {
            TierSelection();
            resources.SpendResources(scrap_cost);
            building_prefab.transform.localPosition = new Vector3(0, 0, 0);
            building = Instantiate(building_prefab, gameObject.transform);
        }
    }

    public virtual void UpdradeFactory()
    {
        if (tier + 1 <= capacitys.Length && !upgrading)
        {
            //Updated scrap cost.
            scrap_cost = scrap_costs[tier - 1];

            if (resources.CanAfford(scrap_cost))
            {
                resources.SpendResources(scrap_cost);
                upgrading = true;
                UpdateTier();
            }
        }
    }

   protected virtual void UpdateTier()
    {
        Destroy(building);
        tier++;
        TierSelection();
        building_prefab.transform.localPosition = new Vector3(0, 0, 0);
        building = Instantiate(building_prefab, gameObject.transform);
        this.GetComponent<BoxCollider>().center = building.transform.Find("EntrancePoint").localPosition;
        upgrading = false;
    }

    //Mouse is storded and turned off to make effetivly inside the building.
    protected virtual void OnTriggerStay(Collider other)
    {
        if (other != null && other.tag == "MouseTemp" && mouse_occupants.Count < capacity)
        {
            other.transform.gameObject.SetActive(false);
            MouseTemp mouse = other.gameObject.GetComponent<MouseTemp>();
            mouse_occupants.Add(mouse);
            mouse.Home = this;
        }
    }

    //checks building rotion to place the mice on the right side to stop mice appaering inside building.
    public void MouseLeave(MouseTemp mouse)
    {
        if (!mouse.Moving)
        {
            float angle = this.transform.eulerAngles.y;

            mouse.Home = null;
            mouse.Collider = false;
            mouse.transform.eulerAngles = new Vector3(0, angle - 90, 0);
            mouse.transform.gameObject.SetActive(true);

            mouse_occupants.Remove(mouse);
        }
    }

    public bool CheckOccupants(MouseTemp mouse)
    {
        return mouse_occupants.Contains(mouse);
    }
    public void PopulateInstanceSaveData(ref building_save_data data)
    {
        data.tier = tier;
        data.mouse_ids = new List<string>();

        for (int i = 0; i < mouse_occupants.Count; i++)
        {
            data.mouse_ids.Add(mouse_occupants[i].Mouse_id);
        }
    }

    // GetVectors Updated by Anthony 23/01/26 
    public Vector2Int GetPosition()
    {
        Transform entrance = this.Building.transform.Find("EntrancePoint");

        Vector2Int building_loc = new Vector2Int((int)entrance.position.x, (int)entrance.position.z);

        return building_loc;
    }
}