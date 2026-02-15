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
    [SerializeField] private DestroyTool destroy_tool;
    private bool path_enabled = false;
    private bool destroy_enabled = false;

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
        if (build_tool != null)
        {
            build_tool.ToggleBuildMode();
        }
    }

    public void NUITogglePathTool()
    {
        if (path_tool != null)
        {
            if (!path_enabled)
            {
                path_tool.SetToolEnabled(true);
                path_enabled = true;
            }
            else
            {
                path_tool.SetToolEnabled(false);
                path_enabled = false;
            }
        }
    }

    public void NUIToggleDestroyTool()
    {
        if (destroy_tool != null)
        {
            if (!destroy_enabled)
            {
               destroy_tool.SetToolEnabled(true);
               destroy_enabled = true;
            }
            else
            {
                destroy_tool.SetToolEnabled(false);
                destroy_enabled = false;
            }
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
