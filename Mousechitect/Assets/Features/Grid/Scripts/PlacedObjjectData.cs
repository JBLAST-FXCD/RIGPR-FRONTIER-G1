using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anthony 9/12/25

/// <summary>
/// Metadata for any placed object (building or path tile).
/// Used by save/load and DestroyTool to restore and remove objects cleanly.
/// </summary>
public class PlacedObjectData : MonoBehaviour
{
    public string unique_id = "";

    public int prefab_index = 0;

    public bool is_path = false;

    // Paths only (useful for pathfinding weighting)
    public float speed_modifier = 0.0f;

    public List<Vector2Int> occupied_cells = new List<Vector2Int>();
}
