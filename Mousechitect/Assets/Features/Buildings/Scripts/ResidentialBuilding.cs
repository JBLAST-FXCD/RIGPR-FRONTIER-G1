using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResidentialBuilding : ParentBuilding
{
    [SerializeField] protected int[] max_quality;

    protected int quality;

    public ResidentialBuilding()
    {
        quality = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void ConstructTier()
    {
        if (tier > 0 && tier <= capacitys.Length)
        {
            building_prefab = building_prefabs[tier - 1];
            capacity = capacitys[tier - 1];
            quality = max_quality[tier - 1];
            building_prefab.transform.localPosition = new Vector3(0, 0, 0);
            building = Instantiate(building_prefab, gameObject.transform);
        }
    }
}
