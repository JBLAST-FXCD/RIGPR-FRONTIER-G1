using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Iain Benner 05/12/2025

/// <summary>
/// Designer can set up mesh and data for tiers of buildings.
/// On start the correct varition of the building is picked.
/// Mice can enter and leave the building.
/// </summary>
public class ParentBuilding : MonoBehaviour
{
    //Varibles for designers to create tiers.
    [SerializeField] protected GameObject[] building_prefabs;
    [SerializeField] protected int[] capacitys;
    [SerializeField] protected int tier;

    //Varibles to select the correct paramiters for the tier.
    protected GameObject building_prefab;
    protected GameObject building;
    protected List<MouseTemp> mouse_occupants;
    protected int capacity;

    public int Tier { get { return tier; } }
    public GameObject Building { get { return building; } }
    public List<MouseTemp> Mouse_occupants { get { return mouse_occupants; } }

    public ParentBuilding()
    {
        building_prefab = null;
        mouse_occupants = new List<MouseTemp>();
        capacity = 0;
    }

    protected virtual void Start()
    {
        ConstructTier();
    }

    protected virtual void Update()
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
    }

    protected virtual void TierSelection()
    {
        building_prefab = building_prefabs[tier - 1];
        capacity = capacitys[tier - 1];
    }

    //The function allows for diffrent varition depending on the designers choise and can be used for when the player upgrades the building.
    protected void ConstructTier()
    {
        if (tier > 0 && tier <= capacitys.Length)
        {
            TierSelection();
            building_prefab.transform.localPosition = new Vector3(0, 0, 0);
            building = Instantiate(building_prefab, gameObject.transform);
        }
    }

    protected virtual void UpdateTier()
    {
        tier++;
        if (tier > 0 && tier <= capacitys.Length)
        {
            Destroy(building);
            TierSelection();
            building_prefab.transform.localPosition = new Vector3(0, 0, 0);
            building = Instantiate(building_prefab, gameObject.transform);
            this.GetComponent<BoxCollider>().center = building.transform.Find("EntrancePoint").localPosition;
        }
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

            mouse.Rigidbody = false;
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
}