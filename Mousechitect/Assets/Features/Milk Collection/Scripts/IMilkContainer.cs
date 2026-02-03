using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// jess @ 03/02/2026
// <summary>
// Interface for milk containers (collectors and tanks).
// </summary>
public interface IMilkContainer
{
    GameObject CONTAINER_GAME_OBJECT { get; }
    int CURRENT_MILK_AMOUNT { get; set; }
    int MAX_MILK_CAPACITY { get; set; }
    bool IS_TANK { get; }
}
