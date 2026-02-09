using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;

public class NewUserInterfaceManager : MonoBehaviour
{
    // Manager for the NUI (New User Interface)

    // --- References ---
    
    [Header("General")]
    [SerializeField] private GameObject path_panel;

    [Header("Build Panel")]
    [SerializeField] private GameObject build_panel;
    [SerializeField] private TextMeshProUGUI category_text;
    [SerializeField] private GameObject[] residential_rows;
    [SerializeField] private GameObject[] industrial_rows;
    [SerializeField] private GameObject[] commercial_rows;
    [SerializeField] private GameObject[] research_rows;

    [Header("Systems")]
    [SerializeField] private BuildingManager building_manager;


    // --- Internal Variables ---

    enum CATEGORY
    {
        CATEGORY_RESIDENTIAL,
        CATEGORY_INDUSTRIAL,
        CATEGORY_COMMERCIAL,
        CATEGORY_RESEARCH,
    }
    private CATEGORY current_category = CATEGORY.CATEGORY_RESIDENTIAL;

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


    // --- Private Functions ---

    private void Start()
    {
        building_manager.UpdateBuildPanel += ClosePanels;
    }

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
                current_category = CATEGORY.CATEGORY_RESEARCH;
                category_text.text = "Research";
                break;
            default:
                current_category = CATEGORY.CATEGORY_RESIDENTIAL;
                category_text.text = "Error";
                break;
        }
        UpdateCategory(previous_category);
    }

    // <-- Joe, 09/02/2005
}
