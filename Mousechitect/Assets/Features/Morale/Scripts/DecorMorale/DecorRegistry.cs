using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anthony - 8/2/2026

/// <summary>
/// Singleton registry of all placed decorations.
/// Stores decorations by grid cell so synergy lookups can be done efficiently
/// (checking only nearby cells instead of scanning every decoration).
/// </summary>
public class DecorRegistry : MonoBehaviour
{
    public static DecorRegistry Instance { get; private set; }

    // All decorations
    private readonly HashSet<Decoration> all = new HashSet<Decoration>();

    // Cell - list of decorations in that cell
    private readonly Dictionary<Vector2Int, List<Decoration>> by_cell = new Dictionary<Vector2Int, List<Decoration>>();

    //Basic singleton setup.Only one DecorRegistry should exist in the scene.
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Registers a decoration into the global list + its current grid cell bucket.
    public void Register(Decoration d)
    {
        if (d == null) return;
        all.Add(d);

        if (!by_cell.TryGetValue(d.grid_cell, out var list))
        {
            list = new List<Decoration>();
            by_cell.Add(d.grid_cell, list);
        }
        list.Add(d);
    }
    
    // Removes a decoration from the global list + cell bucket.
    public void Unregister(Decoration d)
    {
        if (d == null) return;
        all.Remove(d);

        if (by_cell.TryGetValue(d.grid_cell, out var list))
        {
            list.Remove(d);
            // Keep dictionary clean to avoid lots of empty lists over time
            if (list.Count == 0) by_cell.Remove(d.grid_cell);
        }
    }

    // Updates a decoration's cell in the registry.
    // If decor moves or set grid_cell after instantiate
    public void UpdateCell(Decoration d, Vector2Int new_cell)
    {
        if (d == null) return;

        // remove from old
        if (by_cell.TryGetValue(d.grid_cell, out var old_list))
        {
            old_list.Remove(d);
            if (old_list.Count == 0) by_cell.Remove(d.grid_cell);
        }

        d.grid_cell = new_cell;

        // add to new
        if (!by_cell.TryGetValue(d.grid_cell, out var new_list))
        {
            new_list = new List<Decoration>();
            by_cell.Add(d.grid_cell, new_list);
        }
        new_list.Add(d);
    }

    public IReadOnlyCollection<Decoration> GetAll() => all;

    public IEnumerable<Decoration> GetNearby(Vector2Int cell, int radius)
    {
        // Square search. Adapter can do circular cutoff for accuracy.
        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                var c = new Vector2Int(cell.x + dx, cell.y + dy);
                if (by_cell.TryGetValue(c, out var list))
                {
                    for (int i = 0; i < list.Count; i++)
                        yield return list[i];
                }
            }
        }
    }
}

