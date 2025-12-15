using UnityEngine;
using TMPro;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    [Header("Current Resources")]
    public int currentScrap = 100;
    public int currentCheese = 0;

    [Header("UI References")]
    public TextMeshProUGUI scrapText;
    public TextMeshProUGUI cheeseText;

    private void Start()
    {
        UpdateUI();
    }

    //Below functions check if player can afford something and add/deduct cheese and scrap dependant on building production and purchases made.

    public bool CanAfford(int scrapCost, int cheeseCost)
    {
        if (currentScrap >= scrapCost && currentCheese >= cheeseCost)
        {
            return true;
        }
        return false;
    }

    public void SpendResources(int scrapCost, int cheeseCost)
    {
        currentScrap -= scrapCost;
        currentCheese -= cheeseCost;
        UpdateUI();
    }

    public void AddResources(int scrapToAdd, int cheeseToAdd)
    {
        currentScrap += scrapToAdd;
        currentCheese += cheeseToAdd;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scrapText != null) scrapText.text = "Scrap: " + currentScrap;
        if (cheeseText != null) cheeseText.text = "Cheese: " + currentCheese;
    }
}