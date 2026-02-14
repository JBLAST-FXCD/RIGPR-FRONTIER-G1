using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anthony - 2/2/2026
public class FactoryCheeseSwitcherDebug : MonoBehaviour
{
    [SerializeField] private Camera main_camera;
    [SerializeField] private LayerMask factory_mask = ~0; //set to buildings layer

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("[FactorySwitch] L pressed");
            
            FactoryBuilding[] all = GameObject.FindObjectsOfType<FactoryBuilding>();
            Debug.Log($"[FactorySwitch] FactoryBuilding instances in scene: {all.Length}");
            for (int i = 0; i < all.Length; i++)
            {
                Debug.Log($"[FactorySwitch]  #{i} name={all[i].name} id={all[i].GetInstanceID()} parent={all[i].transform.parent?.name}");
            }
            
            TrySwitchHoveredFactory();
        }
    }

    private void Awake()
    {
        if (main_camera == null) main_camera = Camera.main;

        int mask = LayerMask.GetMask("Buildings");
        //Debug.Log($"[FactorySwitch] BuildingTest mask = {mask}");

        factory_mask = mask;
    }


    private void TrySwitchHoveredFactory()
    {
        if (main_camera == null) main_camera = Camera.main;
        if (main_camera == null)
        {
            Debug.LogWarning("[FactorySwitch] No main camera found.");
            return;
        }

        Ray ray = main_camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, factory_mask))
        {
            FactoryBuilding factory = hit.collider.GetComponentInChildren<FactoryBuilding>();
            if (factory != null)
            {
                Debug.Log($"[FactorySwitch] Hit {hit.collider.name}, switching factory {factory.name}");
                factory.CycleCheeseType();
            }
            else
            {
                Debug.Log($"[FactorySwitch] Hit {hit.collider.name} but no FactoryBuilding in parents.");
            }
        }
        else
        {
            Debug.Log("[FactorySwitch] Raycast hit nothing.");
        }
    }
}

