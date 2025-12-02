using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anthony Grummett - 2/12/25

/// <summary>
/// Controls the grid-based building placement flow:
/// Enter / exit build mode
/// Spawn a ghost building
/// Snap it to the grid
/// Rotate buildings with R
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

    private const float COARSE_ROTATION_STEP_DEGREES = 15.0f;
    private const float FINE_ROTATION_STEP_DEGREES = 0.25f;
    private const float ROTATION_HOLD_THRESHOLD_SECONDS = 0.2f;

    private const float RMB_CLICK_MAX_DRAG_DISTANCE = 5.0f;      // pixels
    private const float RMB_CLICK_MAX_DURATION = 0.3f;           // seconds

    private Vector2 rmb_down_position = Vector2.zero;
    private float rmb_down_time = 0.0f;

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

    // Rotation state for the current preview building
    private float current_rotation_y = 0.0f;
    private bool is_fine_rotation_mode = false;
    private bool has_used_fine_rotation = false;
    private float rotation_key_down_time = 0.0f;
    private Quaternion base_building_rotation = Quaternion.identity;

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
            HandleRotationInput();
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

        // reset rotation state for this new preview
        base_building_rotation = current_building.transform.rotation;
        current_rotation_y = 0.0f;
        is_fine_rotation_mode = false;
        has_used_fine_rotation = false;

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

    // ROTATION

    /// <summary>
    /// Handles rotation input for the current preview:
    /// Tap R: rotate 15 degrees.
    /// Hold R: fine rotation at 1 degree per frame.
    /// After fine rotation, the next tap snaps to nearest 15 then rotates 15.
    /// </summary>
    private void HandleRotationInput()
    {
        if (current_building == null)
        {
            return;
        }

        // start tracking when R was pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            rotation_key_down_time = Time.time;
        }

        // while R is held, check if we should enter fine rotation mode
        if (Input.GetKey(KeyCode.R))
        {
            if (!is_fine_rotation_mode)
            {
                float held_time = Time.time - rotation_key_down_time;

                if (held_time >= ROTATION_HOLD_THRESHOLD_SECONDS)
                {
                    is_fine_rotation_mode = true;
                    has_used_fine_rotation = true;
                }
            }

            if (is_fine_rotation_mode)
            {
                current_rotation_y += FINE_ROTATION_STEP_DEGREES;
                NormalizeCurrentRotation();
                ApplyCurrentRotationToBuilding();
            }
        }

        // when R is released, if we never entered fine mode this is a tap
        if (Input.GetKeyUp(KeyCode.R))
        {
            if (!is_fine_rotation_mode)
            {
                // coarse rotation: 15 degree steps
                if (has_used_fine_rotation)
                {
                    // snap to nearest 15 degrees if we previously used fine rotation
                    current_rotation_y = SnapAngleToStep(current_rotation_y, COARSE_ROTATION_STEP_DEGREES);
                }

                current_rotation_y += COARSE_ROTATION_STEP_DEGREES;
                NormalizeCurrentRotation();
                ApplyCurrentRotationToBuilding();
            }

            is_fine_rotation_mode = false;
        }
    }

    /// <summary>
    /// Applies the current Y rotation to the preview, relative to its original prefab rotation.
    /// </summary>
    private void ApplyCurrentRotationToBuilding()
    {
        if (current_building == null)
        {
            return;
        }

        current_building.transform.rotation =
            base_building_rotation * Quaternion.Euler(0.0f, current_rotation_y, 0.0f);
    }

    /// <summary>
    /// Keeps current_rotation_y within 0–360 range.
    /// </summary>
    private void NormalizeCurrentRotation()
    {
        current_rotation_y = Mathf.Repeat(current_rotation_y, 360.0f);
    }

    /// <summary>
    /// Rounds an angle to the nearest multiple of a given step.
    /// </summary>
    private float SnapAngleToStep(float angle, float step_degrees)
    {
        return Mathf.Round(angle / step_degrees) * step_degrees;
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
    /// Right mouse button: drag = camera rotation, short click = cancel preview
    /// Left mouse button: confirm placement if valid
    /// </summary>
    private void HandleMouseInput()
    {
        // Track RMB press for click vs drag detection
        if (Input.GetMouseButtonDown(1))
        {
            rmb_down_position = Input.mousePosition;
            rmb_down_time = Time.time;
        }

        // On RMB release, decide whether this was a "click" (cancel) or a drag (camera only)
        if (Input.GetMouseButtonUp(1))
        {
            Vector2 rmb_up_position = Input.mousePosition;
            float rmb_drag_distance = Vector2.Distance(rmb_down_position, rmb_up_position);
            float rmb_duration = Time.time - rmb_down_time;

            bool is_rmb_click = rmb_drag_distance <= RMB_CLICK_MAX_DRAG_DISTANCE &&
                                rmb_duration <= RMB_CLICK_MAX_DURATION;

            if (is_rmb_click)
            {
                // Treat as cancel placement, without interfering with camera rotation
                CancelCurrentBuilding();
                return;
            }
            // If it was a drag, we do nothing here – camera script handles rotation.
        }

        // LMB - attempt to confirm placement
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