using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Jess Batey 24/02/2026
// <summary>
// Responsible for displaying the UI for a single research node in the research tree, and handling player interaction with it
// </summary>
public class ResearchNodeUI : MonoBehaviour
{
    [Header("data")]
    public UpgradeDefinition research_data;

    [Header("UI References")]
    public Image icon_image;
    public TextMeshProUGUI name_text;
    public TextMeshProUGUI cost_text;
    public Button purchase_button;
    public GameObject locked_overlay;
    public GameObject purchased_checkmark;

    private void Start()
    {
        if (research_data != null)
        {
            name_text.text = research_data.upgrade_name;
            icon_image.sprite = research_data.icon;
            cost_text.text = $"{research_data.scrap_cost} Scrap\n{research_data.cheese_amount} {research_data.type}";

            purchase_button.onClick.AddListener(OnPurchaseClicked);
        }
    }

    private void Update()
    {
        if (research_data == null || ResearchManager.instance == null) return;

        bool is_purchased = ResearchManager.instance.IsUnlocked(research_data.upgrade_id);
        bool prereq_met = research_data.required_prerequisite == null || ResearchManager.instance.IsUnlocked(research_data.required_prerequisite.upgrade_id);

        if (is_purchased)
        {
            purchase_button.interactable = false;
            locked_overlay.SetActive(false);
            purchased_checkmark.SetActive(true);
        }
        else if (prereq_met)
        {
            purchase_button.interactable = true;
            locked_overlay.SetActive(false);
            purchased_checkmark.SetActive(false);
        }
        else
        {
            purchase_button.interactable = false;
            locked_overlay.SetActive(true);
            purchased_checkmark.SetActive(false);
        }
    }

    private void OnPurchaseClicked()
    {
        ResearchManager.instance.AttemptResearch(research_data);
    }
}
