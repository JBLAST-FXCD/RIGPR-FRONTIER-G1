using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NUIConnector : MonoBehaviour
{
    // Script for handling external connections. Improves modularity and implementation.
    // Created by Joe Mcdonnell, 15/02/2026
    
    [Header("Exernal Connections")]
    [SerializeField] private BuildingManager build_tool;
    [SerializeField] private PathTool path_tool;
    [SerializeField] private BuildToolController tool_controller;
    //private bool path_enabled = false;
    //private bool destroy_enabled = false;
    //private bool move_enabled = false;

    [Header("Internal Connections")]
    [SerializeField] private NewUserInterfaceManager nui_manager;



    private void Start()
    {
        build_tool.UpdateBuildPanel += InterceptBroadcast;
    }

    private void InterceptBroadcast()
    {
        nui_manager.ToggleBuildPanel(true);
    }

    public void NUIToggleBuildTool()
    {
        if (tool_controller != null)
        {
            tool_controller.OnBuildingToolButton();
        }
    }

    public void NUITogglePathTool()
    {
        if (tool_controller != null)
        {
            tool_controller.OnPathToolButton();
        }
    }

    public void NUIToggleDestroyTool()
    {
        if (tool_controller != null)
        {
            tool_controller.OnDestroyToolButton();
        }

    }

    public void NUIToggleMoveTool()
    {
        if (tool_controller != null)
        {
            tool_controller.ToggleMoveTool();
        }
    }

    public void NUIPlaceBuilding(int building_index)
    {
        if (build_tool != null)
        {
            build_tool.OnBuildingButtonPressed(building_index);
        }
    }

    public void NUIPlacePath(int path_index)
    {
        if (path_tool != null)
        {
            path_tool.OnPathButtonPressed(path_index);
        }
    }

}
