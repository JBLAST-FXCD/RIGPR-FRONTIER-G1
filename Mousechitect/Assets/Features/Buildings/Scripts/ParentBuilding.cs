using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Iain Benner 05/12/2025

/// <summary>
/// Designer can set up mesh and data for tiers of buildings.
/// On start the correct varition of the building is picked.
/// Mice can enter and leave the building.
/// </summary>
public class ParentBuilding : MonoBehaviour, ISaveable
{
    //Varibles for designers to create tiers.
    [SerializeField] protected GameObject[] building_prefabs;
    [SerializeField] protected int[]        capacitys;
    [SerializeField] protected int          tier;

    //Varibles to select the correct paramiters for the tier.
    protected GameObject      building_prefab;
    protected GameObject      building;
    protected List<MouseTemp> mouse_occupants;
    protected int             capacity;

    public ParentBuilding()
    {
        building_prefab = null;
        mouse_occupants = new List<MouseTemp>();
        capacity = 0;
    }

    void Start()
    {
        ConstructTier();
    }

    protected void Update()
    {
        //This is for debuging.
        //Mise will leave when certain conditions are met, depending on the building type.
        if (Input.GetKeyDown(KeyCode.E))
        {
            MouseLeave(mouse_occupants[0]);
        }

        //This is for debuging.
        //Buildings will be upgraded when certain conditions are met, depending on the building type.
        if (Input.GetKeyDown(KeyCode.Q))
        {
            UpdateTier();
        }
    }

    protected void TierSelection()
    {
        building_prefab = building_prefabs[tier - 1];
        capacity        = capacitys[tier - 1];
    }

    //The funtionb allows for diffrent varition depending on the designers choise and can be used for when the player upgrades the building.
    protected void ConstructTier() 
    {
        if (tier > 0 && tier <= capacitys.Length)
        {
            TierSelection();
            building_prefab.transform.localPosition = new Vector3(0, 0, 0);
            building = Instantiate(building_prefab, gameObject.transform);
        }
    }

    protected void UpdateTier()
    {
        tier++;
        if (tier > 0 && tier <= capacitys.Length)
        {
            Destroy(building);
            TierSelection();
            building_prefab.transform.localPosition = new Vector3(0, 0, 0);
            building = Instantiate(building_prefab, gameObject.transform);
        }
    }

    //Mouse is storded and turned off to make effetivly inside the building.
    protected void OnTriggerEnter(Collider other)
    {
        if (other != null && other.tag == "MouseTemp" && mouse_occupants.Count < capacity)
        {
            other.transform.gameObject.SetActive(false);
            mouse_occupants.Add(other.gameObject.GetComponent<MouseTemp>());
        }
    }

    //checks building rotion to place the mice on the right side to stop mice appaering inside building.
    protected void MouseLeave(MouseTemp mouse)
    {
        Vector3 new_loc = mouse.transform.localPosition;

        switch (this.transform.eulerAngles.y)
        {
            case 0:
                new_loc.z += 1;
                break;
            case 90:
                new_loc.x += 1;
                break;
            case 180:
                new_loc.z -= 1;
                break;
            case 270:
                new_loc.x -= 1;
                break;
        }

        mouse_occupants.Remove(mouse);

        mouse.transform.localPosition = new_loc;
        mouse.transform.gameObject.SetActive(true);
    }

    public void PopulateSaveData(GameData data) 
    {
        data.Parent_Building.building_prefabs = building_prefabs;
        data.Parent_Building.capacitys = capacitys;
        data.Parent_Building.tier = tier;
    }
    public void LoadFromSaveData(GameData data) 
    {
        building_prefabs = data.Parent_Building.building_prefabs;
        capacitys = data.Parent_Building.capacitys;
        tier = data.Parent_Building.tier;

        Start();
    }
}