using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anthony - 15/2/2026
/// <summary>
/// Move Tool:
/// - Toggle with M (via BuildToolController temp until UI implemented)
/// - Click a placed building to select it
/// - Drag it along the grid (snapped)
/// - LMB confirms placement if valid (no overlap)
/// - RMB cancels move and restores original position/cells
/// - holding SHIFT allows you to keep moving buildings 
/// </summary>
public class MoveTool : MonoBehaviour
{
    private const float RAYCAST_MAX_DISTANCE = 5000.0f;
    private const float PREVIEW_OPACITY = 0.55f;
    private const float CELL_BOUNDS_EPSILON = 0.001f;

    [Header("Core References")]
    [SerializeField] private Camera main_camera;
    [SerializeField] private LayerMask move_target_mask;   // buildings layer
    [SerializeField] private LayerMask build_surface_mask; // ground layer
    [SerializeField] private GridManager grid_manager;
    [SerializeField] private BuildingManager building_manager;

    private bool is_tool_active = false;

    // Selection
    private PlacedObjectData selected_placed = null;
    private GameObject selected_root = null;
    private Collider selected_collider = null;
    private BuildingPreviewVisual selected_preview_visual = null;

    // Original state (for cancel)
    private Vector3 original_position;
    private Quaternion original_rotation;
    private readonly List<Vector2Int> original_cells = new List<Vector2Int>();

    // Working footprint
    private readonly List<Vector2Int> candidate_cells = new List<Vector2Int>();
    private bool is_valid_position = false;

    private float reselect_block_timer = 0f;


    public void SetToolEnabled(bool is_enabled)
    {
        enabled = is_enabled;
        is_tool_active = is_enabled;

        // If the controller disables the tool (because another tool is selected),
        // restore the building BUT do not change tool state.
        if (!is_enabled)
        {
            CancelMoveAndRestore(exit_tool: false);
        }
    }


    private void Update()
    {
        if (!is_tool_active) return;

        if (reselect_block_timer > 0f)
        {
            reselect_block_timer -= Time.deltaTime;
        }

        if (Input.GetMouseButtonDown(1) && selected_root != null)
        {
            CancelMoveAndRestore(exit_tool: true);
            reselect_block_timer = 0.15f;
            return;

        }


        if (selected_root == null)
        {
            // Only select on click
            if (Input.GetMouseButtonDown(0))
                TrySelectBuilding();

            return;
        }


        else
        {
            UpdateDragPreview();

            // LMB confirms if valid
            if (Input.GetMouseButtonDown(0) && is_valid_position)
            {
                ConfirmMove();
                reselect_block_timer = 0.15f;

            }
        }
    }

    private void TrySelectBuilding()
    {
        Ray ray = main_camera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, RAYCAST_MAX_DISTANCE, move_target_mask))
            return;

        PlacedObjectData placed = hit.collider.GetComponentInParent<PlacedObjectData>();
        if (placed == null) return;


        selected_placed = placed;
        selected_root = placed.gameObject;

        // Prefer footprint collider on the root
        selected_collider = selected_root.GetComponent<BoxCollider>();

        // Fallback if needed
        if (selected_collider == null)
            selected_collider = selected_root.GetComponentInChildren<Collider>();

        selected_preview_visual = selected_root.GetComponentInChildren<BuildingPreviewVisual>();

        original_position = selected_root.transform.position;
        original_rotation = selected_root.transform.rotation;

        original_cells.Clear();
        if (placed.occupied_cells != null)
            original_cells.AddRange(placed.occupied_cells);

        // Temporarily free its current cells to test overlap properly while moving
        building_manager.ClearOccupiedCells(original_cells);
        grid_manager.SetPathOnCells(original_cells, 0.0f); // clear blocked modifier

        SetBuildingOpacity(selected_root, PREVIEW_OPACITY);
    }

    private void UpdateDragPreview()
    {
        Ray ray = main_camera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, RAYCAST_MAX_DISTANCE, build_surface_mask))
            return;

        Vector3 snapped = grid_manager.GetNearestPointOnGrid(hit.point);
        snapped.y = hit.point.y;

        selected_root.transform.position = snapped;

        if (selected_collider == null)
            return;

        GetCellsForBounds(selected_collider.bounds, candidate_cells);

        if (candidate_cells.Count == 0)
        {
            is_valid_position = false;
            if (selected_preview_visual != null)
                selected_preview_visual.SetPreviewColor(false);
            return;
        }

        is_valid_position = building_manager.AreCellsFree(candidate_cells);

        if (selected_preview_visual != null)
            selected_preview_visual.SetPreviewColor(is_valid_position);


    }

    private void ConfirmMove()
    {
        if (selected_preview_visual != null)
        {
            selected_preview_visual.RestoreOriginalColors();
        }

        SetBuildingOpacity(selected_root, 1.0f);

        // Update placed footprint
        selected_placed.occupied_cells.Clear();
        selected_placed.occupied_cells.AddRange(candidate_cells);

        // Re-occupy in manager + restore blocked cell speed modifier
        building_manager.AddOccupiedCells(candidate_cells);
        grid_manager.SetPathOnCells(candidate_cells, selected_placed.speed_modifier);

        // Re-open entrance cell for pathfinding
        Transform entrance = selected_root.transform.Find("EntrancePoint");
        if (entrance != null)
        {
            Vector2Int entrance_cell = new Vector2Int(
                Mathf.RoundToInt(entrance.position.x),
                Mathf.RoundToInt(entrance.position.z)
            );

            grid_manager.SetPathOnCells(new List<Vector2Int> { entrance_cell }, 1.0f);
        }

        ClearSelectionState();

        // Exit move mode unless SHIFT is held
        bool is_shift_held =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        if (!is_shift_held)
        {
            // Disable tool
            BuildToolController controller = FindObjectOfType<BuildToolController>();
            if (controller != null)
                controller.SetActiveTool_None();
        }
    }

    private void CancelMoveAndRestore(bool exit_tool)
    {
        if (selected_root == null || selected_placed == null)
        {
            ClearSelectionState();
            return;
        }

        // Restore transform
        selected_root.transform.position = original_position;
        selected_root.transform.rotation = original_rotation;

        if (selected_preview_visual != null)
            selected_preview_visual.RestoreOriginalColors();

        SetBuildingOpacity(selected_root, 1.0f);

        // Restore original occupancy
        selected_placed.occupied_cells.Clear();
        selected_placed.occupied_cells.AddRange(original_cells);

        building_manager.AddOccupiedCells(original_cells);
        grid_manager.SetPathOnCells(original_cells, selected_placed.speed_modifier);

        // Restore entrance cell open
        Transform entrance = selected_root.transform.Find("EntrancePoint");
        if (entrance != null)
        {
            Vector2Int entrance_cell = new Vector2Int(
                Mathf.RoundToInt(entrance.position.x),
                Mathf.RoundToInt(entrance.position.z)
            );

            grid_manager.SetPathOnCells(new List<Vector2Int> { entrance_cell }, 1.0f);
        }

        ClearSelectionState();

        // If this cancel happened because the controller disabled the tool,
        // do not change the active tool state
        if (!exit_tool) return;

        bool is_shift_held =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        if (!is_shift_held)
        {
            BuildToolController controller = FindObjectOfType<BuildToolController>();
            if (controller != null)
                controller.SetActiveTool_None();
        }
    }


    private void ClearSelectionState()
    {
        selected_placed = null;
        selected_root = null;
        selected_collider = null;
        selected_preview_visual = null;

        candidate_cells.Clear();
        original_cells.Clear();
        is_valid_position = false;
    }

    private void GetCellsForBounds(Bounds bounds, List<Vector2Int> result)
    {
        result.Clear();

        float grid_size = grid_manager.GridSize;

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

    private void SetBuildingOpacity(GameObject building, float opacity)
    {
        if (building == null) return;

        Renderer[] renderers = building.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j] != null && mats[j].HasProperty("_Color"))
                {
                    Color c = mats[j].color;
                    c.a = opacity;
                    mats[j].color = c;
                }
            }
        }
    }
}
