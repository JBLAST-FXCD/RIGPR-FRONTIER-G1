using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // for Guid

//Anthony Grummett - 2/12/25

/// <summary>
/// Controls the grid-based building placement:
/// Enter / exit build mode
/// Spawn a ghost building
/// Snap it to the grid
/// Rotate buildings with R
/// Check occupied cells
/// Confirm or cancel placement
/// 
/// GDD build mode behaviour:
/// - Build mode entered from UI by selecting a building
/// - Building spawns with reduced opacity + highlight
/// - Player can rotate by 15 degrees using R
/// - LMB places building
/// - Build mode exits on placement unless SHIFT is held
/// - ESC exits build mode
/// - RMB drag = camera rotation, RMB click = cancel preview
/// </summary>
public class BuildingManager : MonoBehaviour, ISaveable
{
    private readonly Dictionary<string, PlacedObjectData> placed_buildings_by_id =
    new Dictionary<string, PlacedObjectData>();

    private const float RAYCAST_MAX_DISTANCE = 5000.0f;
    private const float CELL_BOUNDS_EPSILON = 0.001f;
    private const float PREVIEW_OPACITY = 0.6f;
    private const float PLACED_OPACITY = 1.0f;

    // R rotates building 15 degrees; fine mode is extra polish.
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

    // preview building while the player is choosing a location
    private GameObject current_building;
    private Collider current_building_collider;
    private BuildingPreviewVisual current_preview_visual;

    private bool is_build_mode_active = false;
    private bool is_valid_build_zone = false;

    // Rotation state for the current preview building
    private float current_rotation_y = 0.0f;
    private bool is_fine_rotation_mode = false;
    private bool has_used_fine_rotation = false;
    private float rotation_key_down_time = 0.0f;
    private Quaternion base_building_rotation = Quaternion.identity;

    // Remember which prefab index is currently selected (for UI integration)
    private int selected_building_index = 0;

    // Cells already occupied by placed buildings
    private readonly HashSet<Vector2Int> occupied_cells = new HashSet<Vector2Int>();

    // Cells under the current preview building
    private readonly List<Vector2Int> covered_cells = new List<Vector2Int>();

    // Track placed buildings so we can save/load them.
    private readonly List<PlacedObjectData> placed_buildings = new List<PlacedObjectData>();

    private void Update()
    {
        /* keyboard entry to build mode
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleBuildMode();
        }
        */
        
        // Exit build mode entirely when ESC is pressed
        if (is_build_mode_active && Input.GetKeyDown(KeyCode.Escape))
        {
            CancelBuildingPlacement();
        }

        if (!is_build_mode_active)
        {
            return;
        }
        if (!is_build_mode_active)
        {
            return;
        }

        // press 1 to start placing the first building prefab (instead of UI)
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


    // Enables or disables the building tool.
    public void SetToolEnabled(bool is_enabled)
    {
        // Turn the MonoBehaviour on/off so Update() only runs when this tool is active
        enabled = is_enabled;

        if (is_enabled)
        {
            // Enter build mode for this tool
            BuildMode(true);
        }
        else
        {
            // Cleanly cancel any preview and leave build mode
            CancelBuildingPlacement();
            BuildMode(false);
        }
    }
    public void OnBuildingButtonPressed(int building_index)
    {
        if (!is_build_mode_active)
        {
            Debug.Log("Cannot select building: build mode is not active.");
            return;
        }

        StartPlacingBuilding(building_index);
    }

    public bool GetIsBuildModeActive()
    {
        return is_build_mode_active;
    }

    // Entry point when a building type is selected (from UI or debug hotkey).
    public void StartPlacingBuilding(int building_index)
    {
        if (!is_build_mode_active)
        {
            Debug.Log("StartPlacingBuilding called while build mode is inactive.");
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

        // building spawned with reduced opacity
        SetBuildingOpacity(current_building, PREVIEW_OPACITY);
    }

    // Removes the given cells from the occupied set.
    // Used by DestroyTool when a placed object is removed.
    public void ClearOccupiedCells(List<Vector2Int> cells)
    {
        if (cells == null)
        {
            return;
        }

        int i = 0;

        while (i < cells.Count)
        {
            occupied_cells.Remove(cells[i]);
            ++i;
        }
    }

    // Allows UI / tool controller to cancel only the current preview
    // without exiting build mode entirely.
    public void CancelCurrentBuildingPreview()
    {
        CancelCurrentBuilding();
    }

    // BUILD MODE

    // Toggles the overall build mode state
    public void ToggleBuildMode()
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

    // Enables or disables build mode.
    // When disabled, any active preview is cleared.
    private void BuildMode(bool is_active)
    {
        is_build_mode_active = is_active;

        if (!is_build_mode_active)
        {
            ClearPreview();
        }
    }

    // ROTATION

    // Handles rotation input for the current preview:
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
                // rotation: 15 degree steps
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

    // Applies the current Y rotation to the preview, relative to its original prefab rotation.
    private void ApplyCurrentRotationToBuilding()
    {
        if (current_building == null)
        {
            return;
        }

        current_building.transform.rotation =
            base_building_rotation * Quaternion.Euler(0.0f, current_rotation_y, 0.0f);
    }

    // Keeps current_rotation_y within 0–360 range.
    private void NormalizeCurrentRotation()
    {
        current_rotation_y = Mathf.Repeat(current_rotation_y, 360.0f);
    }

    // Rounds an angle to the nearest multiple of a given step.
    private float SnapAngleToStep(float angle, float step_degrees)
    {
        return Mathf.Round(angle / step_degrees) * step_degrees;
    }

    // PREVIEW + VALIDATION

    // Updates the ghost building position and calculates the cells it covers.
    // Also updates "is_valid_build_zone" based on occupied cells.
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

    // Converts the collider bounds into a list of integer grid cell indices.
    // This defines the "building footprint" on the grid.
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

    // Checks whether any of the candidate cells are already occupied.
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

    // Destroys the current preview building and clears temporary state.
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

    // Handles mouse input while in build mode:
    
    // Note: Camera script locks the mouse to screen centre on RMB.
    // For this reason, RMB cancel is time-based only, not drag-distance based.
    private void HandleMouseInput()
    {
        // Track RMB press start time
        if (Input.GetMouseButtonDown(1))
        {
            rmb_down_time = Time.time;
        }

        // On RMB release, decide whether this was a "click" (cancel) or a hold (camera rotate)
        if (Input.GetMouseButtonUp(1))
        {
            float rmb_duration = Time.time - rmb_down_time;

            bool is_rmb_click = rmb_duration <= RMB_CLICK_MAX_DURATION;

            if (is_rmb_click)
            {
                // Treat as cancel placement
                CancelCurrentBuilding();
                return;
            }

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

    // Cancels only the current ghost building, but keeps build mode active.
    private void CancelCurrentBuilding()
    {
        ClearPreview();
    }

    // Cancels the current building and exits build mode entirely.
    private void CancelBuildingPlacement()
    {
        CancelCurrentBuilding();
        BuildMode(false);
    }

    // Confirms the building placement

    private void ConfirmBuildingPlacement()
    {
        if (current_preview_visual != null)
        {
            current_preview_visual.RestoreOriginalColors();
            current_preview_visual = null;
        }

        SetBuildingOpacity(current_building, 1.0f);

        // Attach / fill footprint data so DestroyTool can free these cells later
        PlacedObjectData placed_data = current_building.GetComponent<PlacedObjectData>();

        if (placed_data == null)
        {
            placed_data = current_building.gameObject.AddComponent<PlacedObjectData>();
        }

        placed_data.is_path = false;
        placed_data.occupied_cells.Clear();

        placed_data.is_path = false;
        placed_data.prefab_index = selected_building_index;

        if (string.IsNullOrEmpty(placed_data.unique_id))
        {
            placed_data.unique_id = Guid.NewGuid().ToString("N");
        }

        int i = 0;

        while (i < covered_cells.Count)
        {
            Vector2Int cell = covered_cells[i];

            placed_data.occupied_cells.Add(cell);
            occupied_cells.Add(cell);
            placed_buildings_by_id[placed_data.unique_id] = placed_data;

            ++i;
        }

        current_building = null;
        current_building_collider = null;
        covered_cells.Clear();

        // exit build mode unless SHIFT is held
        bool is_shift_held =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        if (is_shift_held)
        {
            // Stay in build mode and spawn a new ghost of the same building
            StartPlacingBuilding(selected_building_index);
        }
        else
        {
            // Leave build mode (this will also hide the build panel via BuildModeUI)
            BuildMode(false);
        }
    }

    // HELPERS

    // Sets the alpha channel on all materials for the given building.
    // RGB is left unchanged, preview tint is handled by BuildingPreviewVisual.
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

    public bool RemovePlacedBuildingById(string unique_id)
    {
        if (string.IsNullOrEmpty(unique_id))
        {
            return false;
        }

        PlacedObjectData placed_data;

        if (!placed_buildings_by_id.TryGetValue(unique_id, out placed_data))
        {
            return false;
        }

        // Free cells in grid manager
        ClearOccupiedCells(placed_data.occupied_cells);

        placed_buildings_by_id.Remove(unique_id);

        if (placed_data != null)
        {
            Destroy(placed_data.gameObject);
        }

        return true;
    }

    /// <summary>
    /// Writes all placed buildings into GameData for saving.
    /// </summary>
    public void PopulateSaveData(GameData data)
    {
        if (data == null || data.building_data == null)
        {
            return;
        }

        data.building_data.buildings.Clear();

        foreach (KeyValuePair<string, PlacedObjectData> kvp in placed_buildings_by_id)
        {
            PlacedObjectData placed = kvp.Value;

            if (placed == null)
            {
                continue;
            }

            building_save_data entry = new building_save_data();
            entry.unique_id = placed.unique_id;
            entry.prefab_index = placed.prefab_index;
            entry.position = placed.transform.position;
            entry.rotation = placed.transform.rotation;

            entry.occupied_cells = new List<Vector2Int>();
            int i = 0;

            while (i < placed.occupied_cells.Count)
            {
                entry.occupied_cells.Add(placed.occupied_cells[i]);
                ++i;
            }

            data.building_data.buildings.Add(entry);
        }
    }

    public void LoadFromSaveData(GameData data)
    {
        if (data == null || data.building_data == null || data.building_data.buildings == null)
        {
            return;
        }

        // Clear existing placed buildings
        foreach (KeyValuePair<string, PlacedObjectData> kvp in placed_buildings_by_id)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }

        placed_buildings_by_id.Clear();
        occupied_cells.Clear();

        int b = 0;

        while (b < data.building_data.buildings.Count)
        {
            building_save_data entry = data.building_data.buildings[b];

            if (entry.prefab_index < 0 || entry.prefab_index >= building_prefabs.Length)
            {
                ++b;
                continue;
            }

            GameObject new_building = Instantiate(
                building_prefabs[entry.prefab_index],
                entry.position,
                entry.rotation
            );

            PlacedObjectData placed = new_building.GetComponent<PlacedObjectData>();

            if (placed == null)
            {
                placed = new_building.AddComponent<PlacedObjectData>();
            }

            placed.unique_id = entry.unique_id;
            placed.prefab_index = entry.prefab_index;
            placed.is_path = false;

            placed.occupied_cells.Clear();

            if (entry.occupied_cells != null)
            {
                int i = 0;

                while (i < entry.occupied_cells.Count)
                {
                    Vector2Int cell = entry.occupied_cells[i];
                    placed.occupied_cells.Add(cell);
                    occupied_cells.Add(cell);
                    ++i;
                }
            }

            placed_buildings_by_id[placed.unique_id] = placed;

            ++b;
        }
    }
}
