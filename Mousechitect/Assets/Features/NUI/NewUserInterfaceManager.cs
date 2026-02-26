using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewUserInterfaceManager : MonoBehaviour
{
    // Manager for the NUI (New User Interface)
    // Created by Joe Mcdonnell, 09/02/2026

    // --- References ---

    [Header("General")]
    [SerializeField] private NUIConnector nui_connector;
    [SerializeField] private GameObject path_panel;
    [SerializeField] private GameObject destroy_panel;
    [SerializeField] private GameObject move_panel;
    [SerializeField] private GameObject research_tree_panel;
    [SerializeField] private GameObject info_panel;
    

    [Header("Build Panel")]
    [SerializeField] private GameObject build_panel;
    [SerializeField] private TextMeshProUGUI category_text;
    [SerializeField] private GameObject[] residential_rows;
    [SerializeField] private GameObject[] industrial_rows;
    [SerializeField] private GameObject[] commercial_rows;
    [SerializeField] private GameObject[] research_rows;
    [SerializeField] private GameObject[] decoration_rows;

    [Header("Info Panel")]
    [SerializeField] private GameObject[] pages;
    [SerializeField] private GameObject[] page_title_data;
    [SerializeField] private TextMeshProUGUI page_title;

    [Header("Pause Screen")]
    [SerializeField] private GameObject pause_screen;


    // --- Internal Variables ---

    int current_info_index = 0;

    enum CATEGORY
    {
        CATEGORY_RESIDENTIAL,
        CATEGORY_INDUSTRIAL,
        CATEGORY_COMMERCIAL,
        CATEGORY_RESEARCH,
        CATEGORY_DECORATION,
    }
    private CATEGORY current_category = CATEGORY.CATEGORY_RESIDENTIAL;

    enum OPEN_PANEL
    {
        OPEN_PANEL_BUILD,
        OPEN_PANEL_PATH,
        OPEN_PANEL_DESTROY,
        OPEN_PANEL_MOVE,
        OPEN_PANEL_MICE,
        OPEN_PANEL_BUILDING,
        OPEN_PANEL_RESEARCH_TREE,
        OPEN_PANEL_INFO,
        OPEN_PANEL_PAUSE,
        OPEN_PANEL_NONE
    }
    private OPEN_PANEL open_panel = OPEN_PANEL.OPEN_PANEL_NONE;


    // --- Private Functions ---


    private GameObject[] LoadBuildArray(CATEGORY load_category)
    {
        switch (load_category)
        {
            case CATEGORY.CATEGORY_RESIDENTIAL:
                return residential_rows;
            case CATEGORY.CATEGORY_INDUSTRIAL:
                return industrial_rows;
            case CATEGORY.CATEGORY_COMMERCIAL:
                return commercial_rows;
            case CATEGORY.CATEGORY_RESEARCH:
                return research_rows;
            case CATEGORY.CATEGORY_DECORATION:
                return decoration_rows;
            default:
                return residential_rows;
        }
    }

    private void UpdateCategory(CATEGORY previous)
    {
        GameObject[] selected_rows = LoadBuildArray(previous);

        for (int i = 0; i < selected_rows.Length; i++)
        {
            selected_rows[i].SetActive(false);
        }

        selected_rows = LoadBuildArray(current_category);

        for (int i = 0; i < selected_rows.Length; i++)
        {
            selected_rows[i].SetActive(true);
        }

    }


    // --- Public Functions ---

    public void ClosePanels() // Contains cases for panels that have not been implemented yet
    {
        switch (open_panel)
        {
            case OPEN_PANEL.OPEN_PANEL_BUILD:
                ToggleBuildPanel(false);
                break;
            case OPEN_PANEL.OPEN_PANEL_PATH:
                TogglePathPanel();
                break;
            case OPEN_PANEL.OPEN_PANEL_DESTROY:
                ToggleDestroyPanel();
                break;
            case OPEN_PANEL.OPEN_PANEL_MOVE:
                ToggleMovePanel();
                break;
            case OPEN_PANEL.OPEN_PANEL_RESEARCH_TREE:
                ToggleResearchTreePanel();
                break;
            case OPEN_PANEL.OPEN_PANEL_INFO:
                ToggleInfoPanel();
                break;
            case OPEN_PANEL.OPEN_PANEL_MICE:
                //ToggleMicePanel();
                break;
            case OPEN_PANEL.OPEN_PANEL_BUILDING:
                //ToggleBuildingPanel();
                break;
            case OPEN_PANEL.OPEN_PANEL_PAUSE:
                TogglePauseMenu();
                break;
            default:
                break;
        }

    }

    public void ToggleBuildPanel(bool overide)
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

        if (!overide)
        {
            nui_connector.NUIToggleBuildTool();
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
        nui_connector.NUITogglePathTool();
    }

    public void ToggleDestroyPanel()
    {
        if (open_panel == OPEN_PANEL.OPEN_PANEL_DESTROY)
        {
            destroy_panel.GetComponent<Animator>().SetBool("is_panel_open", false);
            open_panel = OPEN_PANEL.OPEN_PANEL_NONE;
        }
        else
        {
            if (open_panel != OPEN_PANEL.OPEN_PANEL_NONE)
            {
                ClosePanels();
            }
            destroy_panel.GetComponent<Animator>().SetBool("is_panel_open", true);
            open_panel = OPEN_PANEL.OPEN_PANEL_DESTROY;
        }
        nui_connector.NUIToggleDestroyTool();
    }

    public void ToggleMovePanel()
    {
        if (open_panel == OPEN_PANEL.OPEN_PANEL_MOVE)
        {
            move_panel.GetComponent<Animator>().SetBool("is_panel_open", false);
            open_panel = OPEN_PANEL.OPEN_PANEL_NONE;
        }
        else
        {
            if (open_panel != OPEN_PANEL.OPEN_PANEL_NONE)
            {
                ClosePanels();
            }
            move_panel.GetComponent<Animator>().SetBool("is_panel_open", true);
            open_panel = OPEN_PANEL.OPEN_PANEL_MOVE;
        }
        nui_connector.NUIToggleMoveTool();
    }

    public void ToggleResearchTreePanel()
    {
        if (open_panel == OPEN_PANEL.OPEN_PANEL_RESEARCH_TREE)
        {
            research_tree_panel.GetComponent<Animator>().SetBool("is_panel_open", false);
            open_panel = OPEN_PANEL.OPEN_PANEL_NONE;
        }
        else
        {
            if (open_panel != OPEN_PANEL.OPEN_PANEL_NONE)
            {
                ClosePanels();
            }
            research_tree_panel.GetComponent<Animator>().SetBool("is_panel_open", true);
            open_panel = OPEN_PANEL.OPEN_PANEL_RESEARCH_TREE;
        }
    }

    public void ToggleInfoPanel()
    {
        if (open_panel == OPEN_PANEL.OPEN_PANEL_INFO)
        {
            info_panel.GetComponent<Animator>().SetBool("is_panel_open", false);
            open_panel = OPEN_PANEL.OPEN_PANEL_NONE;
        }
        else
        {
            if (open_panel != OPEN_PANEL.OPEN_PANEL_NONE)
            {
                ClosePanels();
            }
            info_panel.GetComponent<Animator>().SetBool("is_panel_open", true);
            open_panel = OPEN_PANEL.OPEN_PANEL_INFO;
        }
    }

    public void TogglePauseMenu()
    {
        if (open_panel == OPEN_PANEL.OPEN_PANEL_PAUSE)
        {
            pause_screen.SetActive(false);
            open_panel = OPEN_PANEL.OPEN_PANEL_NONE;
        }
        else
        {
            if (open_panel != OPEN_PANEL.OPEN_PANEL_NONE)
            {
                ClosePanels();
            }
            pause_screen.SetActive(true);
            open_panel = OPEN_PANEL.OPEN_PANEL_PAUSE;
        }
    }

    public void InfoNext()
    {

        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }

        if (current_info_index + 1 > pages.Length-1)
        {
            current_info_index = 0;
        }
        else
        {
            current_info_index++;
        }

        pages[current_info_index].SetActive(true);
        page_title.text = page_title_data[current_info_index].GetComponent<TextMeshProUGUI>().text;

    }

    public void InfoPrev()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }

        if (current_info_index - 1 < 0)
        {
            current_info_index = pages.Length-1;
        }
        else
        {
            current_info_index--;
        }

        pages[current_info_index].SetActive(true);
        page_title.text = page_title_data[current_info_index].GetComponent<TextMeshProUGUI>().text;
    }

    public void BuildCategoryNext()
    {
        CATEGORY previous_category = current_category;
        switch (current_category)
        {
            case CATEGORY.CATEGORY_RESIDENTIAL:
                current_category = CATEGORY.CATEGORY_INDUSTRIAL;
                category_text.text = "Industrial";
                break;
            case CATEGORY.CATEGORY_INDUSTRIAL:
                current_category = CATEGORY.CATEGORY_COMMERCIAL;
                category_text.text = "Commerical";
                break;
            case CATEGORY.CATEGORY_COMMERCIAL:
                current_category = CATEGORY.CATEGORY_RESEARCH;
                category_text.text = "Research";
                break;
            case CATEGORY.CATEGORY_RESEARCH:
                current_category = CATEGORY.CATEGORY_DECORATION;
                category_text.text = "Decoration";
                break;
            case CATEGORY.CATEGORY_DECORATION:
                current_category = CATEGORY.CATEGORY_RESIDENTIAL;
                category_text.text = "Residential";
                break;
            default:
                current_category = CATEGORY.CATEGORY_RESIDENTIAL;
                category_text.text = "Error";
                break;
        }
        UpdateCategory(previous_category);
    }

    public void BuildCategoryPrevious()
    {
        CATEGORY previous_category = current_category;
        switch (current_category)
        {
            case CATEGORY.CATEGORY_DECORATION:
                current_category = CATEGORY.CATEGORY_RESEARCH;
                category_text.text = "Research";
                break;
            case CATEGORY.CATEGORY_RESEARCH:
                current_category = CATEGORY.CATEGORY_COMMERCIAL;
                category_text.text = "Commerical";
                break;
            case CATEGORY.CATEGORY_COMMERCIAL:
                current_category = CATEGORY.CATEGORY_INDUSTRIAL;
                category_text.text = "Industrial";
                break;
            case CATEGORY.CATEGORY_INDUSTRIAL:
                current_category = CATEGORY.CATEGORY_RESIDENTIAL;
                category_text.text = "Residential";
                break;
            case CATEGORY.CATEGORY_RESIDENTIAL:
                current_category = CATEGORY.CATEGORY_DECORATION;
                category_text.text = "Decoration";
                break;
            default:
                current_category = CATEGORY.CATEGORY_RESIDENTIAL;
                category_text.text = "Error";
                break;
        }
        UpdateCategory(previous_category);
    }

   public void PauseOptionSave()
    {
        nui_connector.CreateSave();
    }
}
