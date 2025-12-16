using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Anthony Grummett - 9/12/25

/// <summary>
/// Handles all build-mode UI:
/// - Shows / hides panels when build mode is active
/// - Switches between Building, Path and Destroy tools
/// - Controls build mode button behaviour
/// </summary>
public class BuildModeUI : MonoBehaviour
{
    private enum TOOL_MODE
    {
        TOOL_MODE_BUILDING,
        TOOL_MODE_PATH,
        TOOL_MODE_DESTROY
    }

    [Header("Core References")]
    [SerializeField] private BuildingManager building_manager;
    [SerializeField] private PathTool path_tool;
    [SerializeField] private DestroyTool destroy_tool;

    [Header("UI Panels")]
    [SerializeField] private GameObject build_panel;          // Panel_Build (Button_House / Cube / Cylinder + hints)
    [SerializeField] private GameObject path_panel;           // Panel_Path (path buttons + hints)
    [SerializeField] private GameObject tool_buttons_panel;   // Panel_ToolButtons (Path_Button Destroy_Button)

    [Header("Hint Panels (optional)")]
    [SerializeField] private GameObject build_hints_panel;    // Panel_BuildHints under Panel_Build
    [SerializeField] private GameObject path_hints_panel;     // Panel_BuildHints under Panel_Path

    private bool is_last_build_mode_state = false;
    private TOOL_MODE current_tool_mode = TOOL_MODE.TOOL_MODE_BUILDING;

    private void Start()
    {
        UpdatePanels(false);
    }

    private void Update()
    {
        if (building_manager == null)
        {
            return;
        }

        bool is_build_mode_active = building_manager.GetIsBuildModeActive();

        if (is_build_mode_active != is_last_build_mode_state)
        {
            UpdatePanels(is_build_mode_active);
            is_last_build_mode_state = is_build_mode_active;
        }
    }

    // PUBLIC

    // Called by BuildMode_Button.
    public void OnBuildModeButtonPressed()
    {
        if (building_manager == null)
        {
            return;
        }

        bool is_build_mode_active = building_manager.GetIsBuildModeActive();

        if (!is_build_mode_active)
        {
            // Turn build mode on via BuildingManager (spawns preview when a building is chosen)
            building_manager.ToggleBuildMode();
            // Update() will pick up the state change and call UpdatePanels(true),
            // which defaults to building UI, but we also force it here.
            ShowBuildingUI();
        }
        else
        {
            if (current_tool_mode == TOOL_MODE.TOOL_MODE_BUILDING)
            {
                // Already in building tool - treat button as "exit build mode"
                building_manager.ToggleBuildMode();
                // UpdatePanels(false) will run from Update().
            }
            else
            {
                // Currently in path or destroy -> just go back to building UI
                ShowBuildingUI();
            }
        }
    }

    // Called by Path_Button in Panel_ToolButtons.
    public void OnPathToolButtonPressed()
    {
        ShowPathUI();
    }

    // Called by Destroy_Button in Panel_ToolButtons.
    public void OnDestroyToolButtonPressed()
    {
        ShowDestroyUI();
    }


    private void UpdatePanels(bool is_active)
    {
        if (!is_active)
        {
            // Hide all build-related UI when build mode is off
            if (build_panel != null)
            {
                build_panel.SetActive(false);
            }

            if (path_panel != null)
            {
                path_panel.SetActive(false);
            }

            if (tool_buttons_panel != null)
            {
                tool_buttons_panel.SetActive(false);
            }

            if (build_hints_panel != null)
            {
                build_hints_panel.SetActive(false);
            }

            if (path_hints_panel != null)
            {
                path_hints_panel.SetActive(false);
            }

            // Disable tools + clear any previews / highlights
            if (path_tool != null)
            {
                path_tool.SetToolEnabled(false);
            }

            if (destroy_tool != null)
            {
                destroy_tool.SetToolEnabled(false);
            }

            if (building_manager != null)
            {
                building_manager.CancelCurrentBuildingPreview();
            }

            current_tool_mode = TOOL_MODE.TOOL_MODE_BUILDING;

            return;
        }

        // Build mode enabled: show tool buttons + default to building UI
        if (tool_buttons_panel != null)
        {
            tool_buttons_panel.SetActive(true);
        }

        ShowBuildingUI();
    }

    private void ShowBuildingUI()
    {
        current_tool_mode = TOOL_MODE.TOOL_MODE_BUILDING;

        if (build_panel != null)
        {
            build_panel.SetActive(true);
        }

        if (path_panel != null)
        {
            path_panel.SetActive(false);
        }

        if (build_hints_panel != null)
        {
            build_hints_panel.SetActive(true);
        }

        if (path_hints_panel != null)
        {
            path_hints_panel.SetActive(false);
        }

        // Only building placement active
        if (path_tool != null)
        {
            path_tool.SetToolEnabled(false);
        }

        if (destroy_tool != null)
        {
            destroy_tool.SetToolEnabled(false);
        }

        if (building_manager != null)
        {
            // Clear any ghost from other tools
            building_manager.CancelCurrentBuildingPreview();
        }
    }

    private void ShowPathUI()
    {
        current_tool_mode = TOOL_MODE.TOOL_MODE_PATH;

        if (build_panel != null)
        {
            build_panel.SetActive(false);
        }

        if (path_panel != null)
        {
            path_panel.SetActive(true);
        }

        if (build_hints_panel != null)
        {
            build_hints_panel.SetActive(false);
        }

        if (path_hints_panel != null)
        {
            path_hints_panel.SetActive(true);
        }

        if (destroy_tool != null)
        {
            destroy_tool.SetToolEnabled(false);
        }

        if (path_tool != null)
        {
            path_tool.SetToolEnabled(true);
        }

        if (building_manager != null)
        {
            building_manager.CancelCurrentBuildingPreview();
        }
    }

    private void ShowDestroyUI()
    {
        current_tool_mode = TOOL_MODE.TOOL_MODE_DESTROY;

        if (build_panel != null)
        {
            build_panel.SetActive(false);
        }

        if (path_panel != null)
        {
            path_panel.SetActive(false);
        }

        if (build_hints_panel != null)
        {
            build_hints_panel.SetActive(false);
        }

        if (path_hints_panel != null)
        {
            path_hints_panel.SetActive(false);
        }

        if (path_tool != null)
        {
            path_tool.SetToolEnabled(false);
        }

        if (destroy_tool != null)
        {
            destroy_tool.SetToolEnabled(true);
        }

        if (building_manager != null)
        {
            building_manager.CancelCurrentBuildingPreview();
        }
    }
}
