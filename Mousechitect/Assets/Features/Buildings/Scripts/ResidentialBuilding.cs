using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Iain Benner 14/12/2025

/// <summary>
/// only difrence from ResidentialBuilding and ParentBuilding is quality matric  
/// </summary>
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

    protected new void TierSelection()
    {
        building_prefab = building_prefabs[tier - 1];
        capacity        = capacitys[tier - 1];
        quality         = max_quality[tier - 1];
    }
}
