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

    public void OnBuildingToolButton()
    {
        SetActiveTool(TOOL_TYPE.TOOL_TYPE_BUILDING);
    }

    public void OnPathToolButton()
    {
        SetActiveTool(TOOL_TYPE.TOOL_TYPE_PATH);
    }

    public void OnDestroyToolButton()
    {
        SetActiveTool(TOOL_TYPE.TOOL_TYPE_DESTROY);
    }

    public void SetActiveTool_None()
    {
        SetActiveTool(TOOL_TYPE.TOOL_TYPE_NONE);
    }

    private void SetActiveTool(TOOL_TYPE new_tool_type)
    {
        current_tool_type = new_tool_type;

        building_tool.SetToolEnabled(current_tool_type == TOOL_TYPE.TOOL_TYPE_BUILDING);
        path_tool.SetToolEnabled(current_tool_type == TOOL_TYPE.TOOL_TYPE_PATH);
        destroy_tool.SetToolEnabled(current_tool_type == TOOL_TYPE.TOOL_TYPE_DESTROY);
        move_tool.SetToolEnabled(current_tool_type == TOOL_TYPE.TOOL_TYPE_MOVE);

    }

    // Temporary key to enter Move Mode
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            // Toggle move tool on/off
            if (current_tool_type == TOOL_TYPE.TOOL_TYPE_MOVE)
                SetActiveTool(TOOL_TYPE.TOOL_TYPE_NONE);
            else
                SetActiveTool(TOOL_TYPE.TOOL_TYPE_MOVE);
        }
    }

}
