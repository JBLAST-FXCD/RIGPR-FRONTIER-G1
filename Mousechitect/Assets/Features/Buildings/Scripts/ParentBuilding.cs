using System.Collections.Generic;
using UnityEngine;

// Iain Benner 05/12/2025

/// <summary>
/// Designer can set up mesh and data for tiers of buildings.
/// On start the correct varition of the building is picked.
/// Mice can enter and leave the building.
/// </summary>
public class parent_building : MonoBehaviour
{
    //Varibles for designers to create tiers.
    [SerializeField] private GameObject[] building_prefabs;
    [SerializeField] private int[]        capacitys;
    [SerializeField] private int          tier;

    //Varibles to select the correct paramiters for the tier.
    private GameObject      building_prefab;
    private GameObject      building;
    private List<MouseTemp> mouse_occupants;
    private int             capacity;

    parent_building()
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

    //The funtionb allows for diffrent varition depending on the designers choise and can be used for when the player upgrades the building.
    private void ConstructTier() 
    {
        if (tier > 0 && tier <= capacitys.Length)
        {
            building_prefab = building_prefabs[tier - 1];
            capacity = capacitys[tier - 1];
            building_prefab.transform.localPosition = new Vector3(0, 0, 0);
            building = Instantiate(building_prefab, gameObject.transform);
        }
    }

    private void UpdateTier()
    {
        tier++;
        if (tier > 0 && tier <= capacitys.Length)
        {
            Destroy(building);
            building_prefab = building_prefabs[tier - 1];
            capacity = capacitys[tier - 1];
            building_prefab.transform.localPosition = new Vector3(0, 0, 0);
            building = Instantiate(building_prefab, gameObject.transform);
        }
    }

    //Mouse is storded and turned off to make effetivly inside the building.
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.tag == "MouseTemp" && mouse_occupants.Count < capacity)
        {
            other.transform.gameObject.SetActive(false);
            mouse_occupants.Add(other.gameObject.GetComponent<MouseTemp>());
        }
    }

    //checks building rotion to place the mice on the right side to stop mice appaering inside building.
    private void MouseLeave(MouseTemp mouse)
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
}