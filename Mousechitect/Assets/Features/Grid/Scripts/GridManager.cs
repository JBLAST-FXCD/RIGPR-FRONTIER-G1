using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles grid snapping and draws the editor gizmo grid.
/// The grid is logical only there are no runtime cell objects.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float grid_size = 1.0f;          // Size of one grid cell (spacing)
    [SerializeField] private Color grid_color = Color.gray;   // Color used for visual gizmos in scene view
    [SerializeField] private int grid_extent = 500;           // How far the grid extends from origin in gizmos

    private const float GRID_Y_POSITION = 0.0f;

    // PUBLIC ACCESSORS

    /// <summary>
    /// Grid cell size used by other systems (e.g. BuildingManager).
    /// </summary>
    public float GridSize
    {
        get { return grid_size; }
    }

    // PUBLIC FUNCTIONS

    /// <summary>
    /// Returns the nearest snapped point on the grid given a world-space position.
    /// </summary>
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

    // PRIVATE FUNCTIONS

    /// <summary>
    /// Draws visible grid lines in the Scene view (editor only).
    /// This is purely visual and has no runtime cost in builds.
    /// </summary>
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