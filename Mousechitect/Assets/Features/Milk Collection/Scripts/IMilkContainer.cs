using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Updated by Iain Benner 20/02/2026
// jess @ 03/02/2026
// <summary>
// Interface for milk containers (collectors and tanks).
// </summary>
public interface IMilkContainer
{
    GameObject CONTAINER_GAME_OBJECT { get; }
    int CURRENT_MILK_AMOUNT { get; set; }
    int[] MAX_MILK_CAPACITYS { get; set; }
    int MAX_MILK_CAPACITY { get; set; }
    BuildingType Building_type { get; }

    bool CanAfford(int MILK);
    void SubtractMilk(int MILK);

    int MilkToAdd();
    void AddMilk(int MILK);
}
