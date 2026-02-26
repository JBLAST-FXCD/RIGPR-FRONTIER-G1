using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Anthony - 7/12/25

public class BuildToolController : MonoBehaviour
{
    private enum TOOL_TYPE
    {
        TOOL_TYPE_NONE,
        TOOL_TYPE_BUILDING,
        TOOL_TYPE_PATH,
        TOOL_TYPE_DESTROY,
        TOOL_TYPE_MOVE
    }

    [SerializeField] private BuildingManager building_tool;
    [SerializeField] private PathTool path_tool;
    [SerializeField] private DestroyTool destroy_tool;
    [SerializeField] private MoveTool move_tool;

    private TOOL_TYPE current_tool_type = TOOL_TYPE.TOOL_TYPE_NONE;

    // --- Broadcast function to communicate to the NUI when a tool disables itself
    public delegate void UpdateNUI(int tool);
    public event UpdateNUI UpdatePanels;
    // --- Added by Joe Mcdonnell, 17/02/2026

    public void OnBuildingToolButton()
    {
        if (current_tool_type == TOOL_TYPE.TOOL_TYPE_BUILDING)
            SetActiveTool(TOOL_TYPE.TOOL_TYPE_NONE);
        else
            SetActiveTool(TOOL_TYPE.TOOL_TYPE_BUILDING);
    }

    public void OnPathToolButton()
    {
        if (current_tool_type == TOOL_TYPE.TOOL_TYPE_PATH)
            SetActiveTool(TOOL_TYPE.TOOL_TYPE_NONE);
        else
            SetActiveTool(TOOL_TYPE.TOOL_TYPE_PATH);
    }

    public void OnDestroyToolButton()
    {
        if (current_tool_type == TOOL_TYPE.TOOL_TYPE_DESTROY)
            SetActiveTool(TOOL_TYPE.TOOL_TYPE_NONE);
        else
            SetActiveTool(TOOL_TYPE.TOOL_TYPE_DESTROY);
    }

    public void SetActiveTool_None()
    {
        //--- Broadcasts to NUIConnector when and which tool has disabled itself
        int tool_num = 0;
        switch(current_tool_type)
        {
            case TOOL_TYPE.TOOL_TYPE_BUILDING:
                tool_num = 1;
                break;
            case TOOL_TYPE.TOOL_TYPE_PATH:
                tool_num = 2;
                break;
            case TOOL_TYPE.TOOL_TYPE_DESTROY:
                tool_num = 3;
                break;
            case TOOL_TYPE.TOOL_TYPE_MOVE:
                tool_num = 4;
                break;
            default:
                tool_num = 0;
                break;
        }
        if (UpdatePanels != null)
        {
            UpdatePanels(tool_num);
        }
        //--- Added by Joe Mcdonnell, 17/02/2026

        SetActiveTool(TOOL_TYPE.TOOL_TYPE_NONE);
    }

    public void ToggleMoveTool()
    {
        if (current_tool_type == TOOL_TYPE.TOOL_TYPE_MOVE)
            SetActiveTool(TOOL_TYPE.TOOL_TYPE_NONE);
        else
            SetActiveTool(TOOL_TYPE.TOOL_TYPE_MOVE);
    }

    private void SetActiveTool(TOOL_TYPE new_tool_type)
    {
        current_tool_type = new_tool_type;

        building_tool.SetToolEnabled(current_tool_type == TOOL_TYPE.TOOL_TYPE_BUILDING);
        path_tool.SetToolEnabled(current_tool_type == TOOL_TYPE.TOOL_TYPE_PATH);
        destroy_tool.SetToolEnabled(current_tool_type == TOOL_TYPE.TOOL_TYPE_DESTROY);
        move_tool.SetToolEnabled(current_tool_type == TOOL_TYPE.TOOL_TYPE_MOVE);

    }

}
