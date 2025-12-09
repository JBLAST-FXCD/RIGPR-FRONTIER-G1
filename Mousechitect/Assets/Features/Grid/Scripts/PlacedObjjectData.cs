using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anthony 9/12/25

/// <summary>
/// Stores grid footprint data for a placed object so DestroyTool (and other tools)
/// can free up the cells when it is removed.
/// </summary>
public class PlacedObjectData : MonoBehaviour
{
    // All grid cells this object currently occupies.
    public List<Vector2Int> occupied_cells = new List<Vector2Int>();

    // Reserved for future use (e.g. distinguishing buildings vs paths).
    public bool is_path = false;
}
