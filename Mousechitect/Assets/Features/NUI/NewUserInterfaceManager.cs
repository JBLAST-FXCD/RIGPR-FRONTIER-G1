using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class NewUserInterfaceManager : MonoBehaviour
{

    [Header("Panels")]
    [SerializeField] private GameObject build_panel;
    [SerializeField] private GameObject path_panel;


    enum OPEN_PANEL
    {
        OPEN_PANEL_BUILD,
        OPEN_PANEL_PATH,
        OPEN_PANEL_DESTROY,
        OPEN_PANEL_MICE,
        OPEN_PANEL_BUILDING,
        OPEN_PANEL_NONE
    }
    private OPEN_PANEL open_panel = OPEN_PANEL.OPEN_PANEL_NONE;

    public void ClosePanels()
    {
        switch (open_panel)
        {
            case OPEN_PANEL.OPEN_PANEL_BUILD:
                ToggleBuildPanel();
                break;
            case OPEN_PANEL.OPEN_PANEL_PATH:
                TogglePathPanel();
                break;
            case OPEN_PANEL.OPEN_PANEL_DESTROY:
                //ToggleDestroyPanel();
                break;
            case OPEN_PANEL.OPEN_PANEL_MICE:
                //ToggleMicePanel();
                break;
            case OPEN_PANEL.OPEN_PANEL_BUILDING:
                //ToggleBuildingPanel();
                break;
            default:
                break;
        }

    }

    public void ToggleBuildPanel()
    {
        if (open_panel == OPEN_PANEL.OPEN_PANEL_BUILD)
        {
            build_panel.GetComponent<Animator>().SetBool("is_panel_open", false);
            open_panel = OPEN_PANEL.OPEN_PANEL_NONE;
        }
        else
        {
            if (open_panel != OPEN_PANEL.OPEN_PANEL_NONE)
            {
                ClosePanels();
            }
            build_panel.GetComponent<Animator>().SetBool("is_panel_open", true);
            open_panel = OPEN_PANEL.OPEN_PANEL_BUILD;

        }

    }

    public void TogglePathPanel()
    {
        if (open_panel == OPEN_PANEL.OPEN_PANEL_PATH)
        {
            path_panel.GetComponent<Animator>().SetBool("is_panel_open", false);
            open_panel = OPEN_PANEL.OPEN_PANEL_NONE;
        }
        else
        {
            if (open_panel != OPEN_PANEL.OPEN_PANEL_NONE)
            {
                ClosePanels();
            }
            path_panel.GetComponent<Animator>().SetBool("is_panel_open", true);
            open_panel = OPEN_PANEL.OPEN_PANEL_PATH;
        }
    }
}
