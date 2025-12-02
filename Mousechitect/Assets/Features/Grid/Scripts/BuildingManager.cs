using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the grid-based building placement flow:
/// Enter / exit build mode
/// Spawn a ghost building
/// Snap it to the grid
/// Check occupied cells
/// Confirm or cancel placement
/// 
/// This script implements the three flowcharts:
/// Main building placement loop
/// BuildMode(bool) behaviour
/// CheckCellSurfaces(cells) style validation
/// </summary>
public class BuildingManager : MonoBehaviour
{
    private const float RAYCAST_MAX_DISTANCE = 5000.0f;
    private const float CELL_BOUNDS_EPSILON = 0.001f;
    private const float PREVIEW_OPACITY = 0.6f;
    private const float PLACED_OPACITY = 1.0f;

    [Header("Core References")]
    [SerializeField] private Camera main_camera;
    [SerializeField] private GridManager grid_manager;
    [SerializeField] private LayerMask build_surface_mask;

    [Header("Building Prefabs")]
    [SerializeField] private GameObject[] building_prefabs;

    // Active ghost / preview building while the player is choosing a location
    private GameObject current_building;
    private Collider current_building_collider;
    private BuildingPreviewVisual current_preview_visual;

    // Flow control flags
    private bool is_build_mode_active = false;
    private bool is_valid_build_zone = false;

    // Remember which prefab index is currently selected (for UI integration later)
    private int selected_building_index = 0;

    // Cells already occupied by placed buildings
    private readonly HashSet<Vector2Int> occupied_cells = new HashSet<Vector2Int>();

    // Cells under the current preview building
    private readonly List<Vector2Int> covered_cells = new List<Vector2Int>();

    private void Update()
    {
        // player can toggle build mode with B key
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildMode();
        }

        if (!is_build_mode_active)
        {
            return;
        }

        // press 1 to start placing the first building prefab
        // Later this will also be driven by UI buttons.
        if (current_building == null && Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartPlacingBuilding(0);
        }

        if (current_building != null)
        {
            UpdatePreviewPositionAndCells();
            HandleMouseInput();
        }
    }

    // PUBLIC FUNCTIONS

    /// <summary>
    /// Entry point when a building type is selected from UI.
    /// Matches "Building selected -> Instantiate building -> BuildMode(true)" in the flowchart.
    /// </summary>
    public void StartPlacingBuilding(int building_index)
    {
        if (!is_build_mode_active)
        {
            Debug.Log("Press B to enter build mode before selecting a building.");
            return;
        }

        if (building_index < 0 || building_index >= building_prefabs.Length)
        {
            return;
        }

        selected_building_index = building_index;

        if (current_building != null)
        {
            Destroy(current_building);
        }

        GameObject building_prefab = building_prefabs[selected_building_index];

        current_building = Instantiate(building_prefab);
        current_building_collider = current_building.GetComponentInChildren<Collider>();
        current_preview_visual = current_building.GetComponentInChildren<BuildingPreviewVisual>();

        SetBuildingOpacity(current_building, PREVIEW_OPACITY);
    }

    // BUILD MODE

    /// <summary>
    /// Toggles the overall build mode state.
    /// </summary>
    private void ToggleBuildMode()
    {
        if (is_build_mode_active)
        {
            CancelBuildingPlacement();
        }
        else
        {
            BuildMode(true);
        }
    }

    /// <summary>
    /// Enables or disables build mode.
    /// When disabled, any active preview is cleared.
    /// </summary>
    private void BuildMode(bool is_active)
    {
        is_build_mode_active = is_active;

        if (!is_build_mode_active)
        {
            ClearPreview();
        }
    }

    // PREVIEW + VALIDATION

    /// <summary>
    /// Updates the ghost building position and calculates the cells it covers.
    /// Also updates "is_valid_build_zone" based on occupied cells.
    /// </summary>
    private void UpdatePreviewPositionAndCells()
    {
        Ray mouse_ray = main_camera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(mouse_ray, out RaycastHit hit_info, RAYCAST_MAX_DISTANCE, build_surface_mask))
        {
            return;
        }

        Vector3 hit_position = hit_info.point;

        // Mouse position - relative cell - snap origin to grid
        Vector3 snapped_position = grid_manager.GetNearestPointOnGrid(hit_position);
        snapped_position.y = hit_info.point.y;

        current_building.transform.position = snapped_position;

        if (current_building_collider == null)
        {
            return;
        }

        Bounds building_bounds = current_building_collider.bounds;

        // Building surface area - calculate cells covered by building
        GetCellsForBounds(building_bounds, covered_cells);

        // CheckCellSurfaces(cells) - returns new is_valid_build_zone
        is_valid_build_zone = CheckCellSurfaces(covered_cells);

        if (current_preview_visual != null)
        {
            current_preview_visual.SetPreviewColor(is_valid_build_zone);
        }
    }

    /// <summary>
    /// Converts the collider bounds into a list of integer grid cell indices.
    /// This defines the "building footprint" on the grid.
    /// </summary>
    private void GetCellsForBounds(Bounds bounds, List<Vector2Int> result)
    {
        result.Clear();

        float grid_size = grid_manager.GridSize;

        // Shrink bounds slightly to avoid an extra cell due to floating point precision.
        float min_x = bounds.min.x + CELL_BOUNDS_EPSILON;
        float max_x = bounds.max.x - CELL_BOUNDS_EPSILON;
        float min_z = bounds.min.z + CELL_BOUNDS_EPSILON;
        float max_z = bounds.max.z - CELL_BOUNDS_EPSILON;

        int cell_min_x = Mathf.FloorToInt(min_x / grid_size);
        int cell_max_x = Mathf.FloorToInt(max_x / grid_size);
        int cell_min_z = Mathf.FloorToInt(min_z / grid_size);
        int cell_max_z = Mathf.FloorToInt(max_z / grid_size);

        for (int x = cell_min_x; x <= cell_max_x; ++x)
        {
            for (int z = cell_min_z; z <= cell_max_z; ++z)
            {
                result.Add(new Vector2Int(x, z));
            }
        }
    }

    /// <summary>
    /// Checks whether any of the candidate cells are already occupied.
    /// Equivalent to the "CheckCellSurfaces(cells)" flowchart.
    /// </summary>
    private bool CheckCellSurfaces(List<Vector2Int> cells)
    {
        bool is_valid = true;

        int i = 0;

        while (i < cells.Count)
        {
            if (occupied_cells.Contains(cells[i]))
            {
                is_valid = false;
                break;
            }

            ++i;
        }

        return is_valid;
    }

    /// <summary>
    /// Destroys the current preview building and clears temporary state.
    /// </summary>
    private void ClearPreview()
    {
        if (current_building != null)
        {
            Destroy(current_building);
        }

        current_building = null;
        current_building_collider = null;
        current_preview_visual = null;

        covered_cells.Clear();
    }

    // INPUT HANDLING

    /// <summary>
    /// Handles mouse input while in build mode:
    /// Right-click cancels the current preview.
    /// Left-click attempts to confirm placement if the zone is valid.
    /// </summary>
    private void HandleMouseInput()
    {
        // RMB - "Building placement cancelled" - branch
        if (Input.GetMouseButtonDown(1))
        {
            CancelCurrentBuilding();
            return;
        }

        // LMB - check if placement is valid - confirm branch
        if (Input.GetMouseButtonDown(0))
        {
            if (!is_valid_build_zone)
            {
                return;
            }

            ConfirmBuildingPlacement();
        }
    }

    /// <summary>
    /// Cancels only the current ghost building, but keeps build mode active.
    /// </summary>
    private void CancelCurrentBuilding()
    {
        ClearPreview();
    }

    /// <summary>
    /// Cancels the current building and exits build mode entirely.
    /// </summary>
    private void CancelBuildingPlacement()
    {
        CancelCurrentBuilding();
        BuildMode(false);
    }

    /// <summary>
    /// Confirms the building placement.
    /// Marks the footprint cells as occupied and restores the original visuals.
    /// </summary>
    private void ConfirmBuildingPlacement()
    {
        if (current_preview_visual != null)
        {
            current_preview_visual.RestoreOriginalColors();
            current_preview_visual = null;
        }

        SetBuildingOpacity(current_building, PLACED_OPACITY);

        int i = 0;

        while (i < covered_cells.Count)
        {
            occupied_cells.Add(covered_cells[i]);
            ++i;
        }

        current_building = null;
        current_building_collider = null;
        covered_cells.Clear();

        // We deliberately stay in build mode so the player can place more of the same type.
        // Call BuildMode(false) here for single placement instead.
    }

    // HELPERS

    /// <summary>
    /// Sets the alpha channel on all materials for the given building.
    /// RGB is left unchanged, preview tint is handled by BuildingPreviewVisual.
    /// </summary>
    private void SetBuildingOpacity(GameObject building, float opacity)
    {
        if (building == null)
        {
            return;
        }

        Renderer[] renderers = building.GetComponentsInChildren<Renderer>();

        int i = 0;

        while (i < renderers.Length)
        {
            Material[] materials = renderers[i].materials;

            int j = 0;

            while (j < materials.Length)
            {
                Material material = materials[j];

                if (material.HasProperty("_Color"))
                {
                    Color color = material.color;
                    color.a = opacity;
                    material.color = color;
                }

                ++j;
            }

            ++i;
        }
    }
}