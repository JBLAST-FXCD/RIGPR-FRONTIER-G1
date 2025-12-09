using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anthony - 7/12/25

/// <summary>
/// Grid-based placement tool for paths.
/// - Uses the same grid as buildings
/// - Spawns a ghost path tile that follows the mouse
/// - Snaps to 1x1 grid cells
/// - Rotates in 90 degree steps with R (for corners)
/// - Left mouse places a path and keeps the tool active
/// - Right mouse quick tap cancels the current preview
/// 
/// </summary>
public class PathTool : MonoBehaviour
{
    [System.Serializable]
    public struct PathTypeData
    {
        public string path_name;       // for inspector clarity only
        public GameObject path_prefab;
        public float speed_modifier;   // added to default floor speed for pathfinding
    }

    private const float RAYCAST_MAX_DISTANCE = 5000.0f;
    private const float CELL_BOUNDS_EPSILON = 0.001f;
    private const float PREVIEW_OPACITY = 0.6f;
    private const float PLACED_OPACITY = 1.0f;

    // 90-degree turns for corners
    private const float PATH_ROTATION_STEP_DEGREES = 90.0f;

    // Time-based RMB click detection (camera warps cursor to centre)
    private const float RMB_CLICK_MAX_DURATION = 0.3f; // seconds

    [Header("Core References")]
    [SerializeField] private Camera main_camera;
    [SerializeField] private GridManager grid_manager;
    [SerializeField] private LayerMask path_surface_mask;

    [Header("Path Types")]
    [SerializeField] private PathTypeData[] path_types;

    // Current preview path
    private GameObject current_path;
    private Collider current_path_collider;
    private BuildingPreviewVisual current_preview_visual;

    private bool is_tool_active = false;
    private bool is_valid_path_zone = false;

    private int selected_path_index = 0;

    private float current_rotation_y = 0.0f;
    private Quaternion base_path_rotation = Quaternion.identity;

    // Cells already occupied by placed paths (for now this only checks against other paths)
    private readonly HashSet<Vector2Int> occupied_path_cells = new HashSet<Vector2Int>();

    // Cells under the current preview
    private readonly List<Vector2Int> covered_cells = new List<Vector2Int>();

    // Optional: per-cell speed modifiers for future pathfinding
    private readonly Dictionary<Vector2Int, float> cell_speed_modifiers =
        new Dictionary<Vector2Int, float>();

    private float rmb_down_time = 0.0f;

    // PUBLIC

    // Enables or disables the path tool.
    public void SetToolEnabled(bool is_enabled)
    {
        enabled = is_enabled;
        is_tool_active = is_enabled;

        if (!is_enabled)
        {
            CancelCurrentPath();
        }
    }

    // Called by a UI button to select a path type by index.
    public void OnPathButtonPressed(int path_index)
    {
        if (!is_tool_active)
        {
            Debug.Log("Cannot select path: path tool is not active.");
            return;
        }

        StartPlacingPath(path_index);
    }

    // Returns the movement speed modifier for a given cell.
    // Pathfinding can combine this with a default floor speed.
    public float GetCellSpeedModifier(Vector2Int cell)
    {
        float modifier;

        if (cell_speed_modifiers.TryGetValue(cell, out modifier))
        {
            return modifier;
        }

        return 0.0f;
    }

    private void Update()
    {
        if (!is_tool_active)
        {
            return;
        }

        // (Optional) ESC to clear current preview, but keep tool active
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelCurrentPath();
        }

        if (current_path != null)
        {
            HandleRotationInput();
            UpdatePreviewPositionAndCells();
            HandleMouseInput();
        }
    }

    // TOOL FLOW

    private void StartPlacingPath(int path_index)
    {
        if (path_index < 0 || path_index >= path_types.Length)
        {
            Debug.LogWarning("Path index out of range.");
            return;
        }

        selected_path_index = path_index;

        PathTypeData path_type = path_types[selected_path_index];

        SpawnPreview(path_type.path_prefab);
    }

    private void SpawnPreview(GameObject path_prefab)
    {
        if (current_path != null)
        {
            Destroy(current_path);
        }

        current_path = Instantiate(path_prefab);
        current_path_collider = current_path.GetComponentInChildren<Collider>();
        current_preview_visual = current_path.GetComponentInChildren<BuildingPreviewVisual>();

        base_path_rotation = current_path.transform.rotation;
        current_rotation_y = 0.0f;

        SetPathOpacity(current_path, PREVIEW_OPACITY);
    }

    private void CancelCurrentPath()
    {
        if (current_path != null)
        {
            Destroy(current_path);
        }

        current_path = null;
        current_path_collider = null;
        current_preview_visual = null;

        covered_cells.Clear();
    }

    private void ConfirmPlacement()
    {
        if (current_preview_visual != null)
        {
            current_preview_visual.RestoreOriginalColors();
            current_preview_visual = null;
        }

        SetPathOpacity(current_path, PLACED_OPACITY);

        PathTypeData path_type = path_types[selected_path_index];

        // Mark cells as occupied and store their speed modifiers
        int i = 0;

        while (i < covered_cells.Count)
        {
            Vector2Int cell = covered_cells[i];

            occupied_path_cells.Add(cell);
            cell_speed_modifiers[cell] = path_type.speed_modifier;

            ++i;
        }

        current_path = null;
        current_path_collider = null;
        covered_cells.Clear();

        // Paths: always keep tool active and spawn a new preview of the same type
        StartPlacingPath(selected_path_index);
    }

    // ROTATION

    private void HandleRotationInput()
    {
        if (current_path == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            current_rotation_y += PATH_ROTATION_STEP_DEGREES;
            current_rotation_y = Mathf.Repeat(current_rotation_y, 360.0f);
            ApplyCurrentRotationToPath();
        }
    }

    private void ApplyCurrentRotationToPath()
    {
        if (current_path == null)
        {
            return;
        }

        current_path.transform.rotation =
            base_path_rotation * Quaternion.Euler(0.0f, current_rotation_y, 0.0f);
    }

    // PREVIEW + VALIDATION

    private void UpdatePreviewPositionAndCells()
    {
        Ray mouse_ray = main_camera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(mouse_ray, out RaycastHit hit_info, RAYCAST_MAX_DISTANCE, path_surface_mask))
        {
            return;
        }

        Vector3 hit_position = hit_info.point;

        Vector3 snapped_position = grid_manager.GetNearestPointOnGrid(hit_position);
        snapped_position.y = hit_info.point.y;

        current_path.transform.position = snapped_position;

        if (current_path_collider == null)
        {
            return;
        }

        Bounds bounds = current_path_collider.bounds;

        GetCellsForBounds(bounds, covered_cells);

        is_valid_path_zone = CheckCellsFree(covered_cells);

        if (current_preview_visual != null)
        {
            current_preview_visual.SetPreviewColor(is_valid_path_zone);
        }
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

    private bool CheckCellsFree(List<Vector2Int> cells)
    {
        bool is_free = true;

        int i = 0;

        while (i < cells.Count)
        {
            if (occupied_path_cells.Contains(cells[i]))
            {
                is_free = false;
                break;
            }

            ++i;
        }

        return is_free;
    }

    // INPUT

    private void HandleMouseInput()
    {
        // RMB quick tap cancels the preview.
        if (Input.GetMouseButtonDown(1))
        {
            rmb_down_time = Time.time;
        }

        if (Input.GetMouseButtonUp(1))
        {
            float rmb_duration = Time.time - rmb_down_time;

            bool is_rmb_click = rmb_duration <= RMB_CLICK_MAX_DURATION;

            if (is_rmb_click)
            {
                CancelCurrentPath();
                return;
            }
        }

        // LMB places a path tile if valid
        if (Input.GetMouseButtonDown(0))
        {
            if (!is_valid_path_zone)
            {
                return;
            }

            ConfirmPlacement();
        }
    }

    // HELPERS

    private void SetPathOpacity(GameObject path_object, float opacity)
    {
        if (path_object == null)
        {
            return;
        }

        Renderer[] renderer_array = path_object.GetComponentsInChildren<Renderer>();

        int i = 0;

        while (i < renderer_array.Length)
        {
            Material[] material_array = renderer_array[i].materials;

            int j = 0;

            while (j < material_array.Length)
            {
                Material material = material_array[j];

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
