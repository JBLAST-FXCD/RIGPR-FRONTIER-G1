using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anthony Grummett - 2/12/25

/// <summary>
/// Handles grid snapping and draws the editor gizmo grid.
/// The grid is logical only; there are no runtime cell objects.
/// Also stores per-cell movement speed modifiers for pathfinding.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float grid_size = 1.0f;          // Size of one grid cell (spacing)
    [SerializeField] private Color grid_color = Color.gray;   // Color used for visual gizmos in scene view
    [SerializeField] private int grid_extent = 500;           // How far the grid extends from origin in gizmos

    [Header("Movement Settings")]
    [SerializeField] private float default_move_speed = 1.0f; // Base movement speed on plain floor

    private const float GRID_Y_POSITION = 0.0f;

    // Per-cell speed modifiers (e.g. paths faster/slower than default).
    // Pathfinding will use: default_move_speed + modifier.
    private readonly Dictionary<Vector2Int, float> cell_speed_modifiers =
        new Dictionary<Vector2Int, float>();

    // PUBLIC ACCESSORS

    // Grid cell size used by other systems (e.g. BuildingManager).
    public float GridSize
    {
        get { return grid_size; }
    }

    // Base movement speed before any path modifiers.
    public float DefaultMoveSpeed
    {
        get { return default_move_speed; }
    }

    // PUBLIC FUNCTIONS

    // Returns the nearest snapped point on the grid given a world-space position.
    public Vector3 GetNearestPointOnGrid(Vector3 position)
    {
        // Snap each axis relative to the grid origin (this GameObject)
        Vector3 local_position = position - transform.position;

        float x_count = Mathf.Round(local_position.x / grid_size);
        float y_count = Mathf.Round(local_position.y / grid_size);
        float z_count = Mathf.Round(local_position.z / grid_size);

        Vector3 result = new Vector3(
            x_count * grid_size,
            y_count * grid_size,
            z_count * grid_size
        );

        // Return back to world space
        return result + transform.position;
    }

    // Sets a movement speed modifier for all given cells.
    // Pathfinding will treat those cells as (default_move_speed + speed_modifier).
    public void SetPathOnCells(List<Vector2Int> cells, float speed_modifier)
    {
        int i = 0;

        while (i < cells.Count)
        {
            cell_speed_modifiers[cells[i]] = speed_modifier;
            ++i;
        }
    }

    //Anthony 7/12/25

    // Returns the final movement speed for a given cell.
    // If there is no path modifier, returns the default speed.
    public float GetCellMoveSpeed(Vector2Int cell)
    {
        float speed_modifier;

        if (cell_speed_modifiers.TryGetValue(cell, out speed_modifier))
        {
            return default_move_speed + speed_modifier;
        }

        return default_move_speed;
    }

    // PRIVATE FUNCTIONS

    // Draws visible grid lines in the Scene view (editor only).
    // This is purely visual and has no runtime cost in builds.
    private void OnDrawGizmos()
    {
        Gizmos.color = grid_color;

        // Draw grid lines centered on the object’s transform.
        // We only draw in XZ as Y is assumed to be "up".
        for (float x = -grid_extent; x <= grid_extent; x += grid_size)
        {
            Vector3 start = new Vector3(x, GRID_Y_POSITION, -grid_extent) + transform.position;
            Vector3 end = new Vector3(x, GRID_Y_POSITION, grid_extent) + transform.position;
            Gizmos.DrawLine(start, end);
        }

        for (float z = -grid_extent; z <= grid_extent; z += grid_size)
        {
            Vector3 start = new Vector3(-grid_extent, GRID_Y_POSITION, z) + transform.position;
            Vector3 end = new Vector3(grid_extent, GRID_Y_POSITION, z) + transform.position;
            Gizmos.DrawLine(start, end);
        }
    }
}