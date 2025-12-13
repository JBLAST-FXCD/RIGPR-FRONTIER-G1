using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anthony 8/12/25

/// <summary>
/// Destroy tool:
/// - Highlights any placed object under the mouse (yellow tint)
/// - On LMB, removes the object and frees its occupied cells in BuildingManager
/// - Designed to be enabled/disabled by a BuildToolController via SetToolEnabled()
/// </summary>
public class DestroyTool : MonoBehaviour
{
    private const float RAYCAST_MAX_DISTANCE = 5000.0f;

    [Header("Core References")]
    [SerializeField] private Camera main_camera;
    [SerializeField] private LayerMask destroy_target_mask;
    [SerializeField] private BuildingManager building_manager;
    [SerializeField] private PathTool path_tool;

    private bool is_tool_active = false;

    // Current hovered target
    private GameObject current_target = null;
    private Renderer[] current_target_renderers = null;
    private Color[][] current_target_original_colors = null;

    // PUBLIC

    // Enables or disables the destroy tool.
    public void SetToolEnabled(bool is_enabled)
    {
        enabled = is_enabled;
        is_tool_active = is_enabled;

        if (!is_enabled)
        {
            ClearHighlight();
        }
    }


    private void Update()
    {
        if (!is_tool_active)
        {
            return;
        }

        UpdateHoverTarget();
        HandleMouseInput();
    }

    // HOVER / HIGHLIGHT

    private void UpdateHoverTarget()
    {
        Ray mouse_ray = main_camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(mouse_ray, out RaycastHit hit_info, RAYCAST_MAX_DISTANCE, destroy_target_mask))
        {
            // We expect placed buildings/paths to have PlacedObjectData somewhere in their hierarchy.
            PlacedObjectData placed_data = hit_info.collider.GetComponentInParent<PlacedObjectData>();

            if (placed_data != null)
            {
                GameObject target_root = placed_data.gameObject;

                if (current_target != target_root)
                {
                    ClearHighlight();
                    SetHighlight(target_root);
                }

                return;
            }
        }

        // Nothing valid under mouse
        ClearHighlight();
    }

    private void SetHighlight(GameObject target)
    {
        current_target = target;
        current_target_renderers = current_target.GetComponentsInChildren<Renderer>();

        current_target_original_colors = new Color[current_target_renderers.Length][];

        for (int r = 0; r < current_target_renderers.Length; ++r)
        {
            Material[] material_array = current_target_renderers[r].materials;
            current_target_original_colors[r] = new Color[material_array.Length];

            for (int m = 0; m < material_array.Length; ++m)
            {
                Material material = material_array[m];

                if (!material.HasProperty("_Color"))
                {
                    continue;
                }

                // Store original colour and then tint to yellow (keep alpha)
                Color original_color = material.color;
                current_target_original_colors[r][m] = original_color;

                Color highlight_color = Color.yellow;
                highlight_color.a = original_color.a;
                material.color = highlight_color;
            }
        }
    }

    private void ClearHighlight()
    {
        if (current_target == null || current_target_renderers == null || current_target_original_colors == null)
        {
            current_target = null;
            current_target_renderers = null;
            current_target_original_colors = null;
            return;
        }

        for (int r = 0; r < current_target_renderers.Length; ++r)
        {
            Material[] material_array = current_target_renderers[r].materials;

            for (int m = 0; m < material_array.Length; ++m)
            {
                if (!material_array[m].HasProperty("_Color"))
                {
                    continue;
                }

                material_array[m].color = current_target_original_colors[r][m];
            }
        }

        current_target = null;
        current_target_renderers = null;
        current_target_original_colors = null;
    }

    // INPUT

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0) && current_target != null)
        {
            PlacedObjectData placed_data = current_target.GetComponent<PlacedObjectData>();

            if (placed_data == null)
            {
                return;
            }

            if (placed_data.is_path)
            {
                if (path_tool != null)
                {
                    path_tool.RemovePlacedPathById(placed_data.unique_id);
                }
            }
            else
            {
                if (building_manager != null)
                {
                    building_manager.RemovePlacedBuildingById(placed_data.unique_id);
                }
            }

            ClearHighlight();
        }
    }
}
