using UnityEngine;
using System.Collections;

/// <summary>
/// Manages all upgrades for residential buildings.
/// Handles morale boosting, the population cap on each residential building type, unlockables and passive cheese generation.
/// </summary>
public class ResidentialUpgradeHandler : BuildingUpgradeHandler
{
    [Header("Base Stats (Only Editable In-Script)")]
    public float currentMoraleMultiplier = 1.0f;
    public int globalPopulationCapBonus = 0;
    public bool rapidConstructionActive = false;

    [Header("Configurables - RES Building Bonuses")]
    [SerializeField] private float moraleBonusSmall = 0.10f;
    [SerializeField] private float moraleBonusLarge = 0.15f;
    [SerializeField] private int neighborPopBonusSmall = 1;
    [SerializeField] private int neighborPopBonusLarge = 3;

    [Header("Configurables - RES Passive Bonuses")]
    [SerializeField] private int tellyCheeseAmount = 5;
    [SerializeField] private float tellyIntervalMinutes = 15f;
    [SerializeField] private int bigCheeseAmount = 10;
    [SerializeField] private float bigCheeseIntervalMinutes = 10f;

    protected override void ApplyUpgradeEffect(UpgradeDefinition upgrade)
    {
        switch (upgrade.upgradeID)
        {
            case "RES_2_1":
                currentMoraleMultiplier += moraleBonusSmall;
                break;
            case "RES_2_2":
                globalPopulationCapBonus += neighborPopBonusSmall;
                break;
            case "RES_2_3":
                StartCoroutine(PassiveCheeseRoutine(tellyCheeseAmount, tellyIntervalMinutes));
                break;
            case "RES_3":
                BuildingUnlockManager.Instance.UnlockBuilding("MilkCartonManor");
                break;
            case "RES_4_1":
                currentMoraleMultiplier += moraleBonusLarge;
                break;
            case "RES_4_2":
                globalPopulationCapBonus += neighborPopBonusLarge;
                break;
            case "RES_4_3":
                StartCoroutine(PassiveCheeseRoutine(bigCheeseAmount, bigCheeseIntervalMinutes));
                break;
            case "RES_5":
                rapidConstructionActive = true;
                break;
            case "RES_6":
                BuildingUnlockManager.Instance.UnlockBuilding("LunchBoxFlats");
                break;
        }
    }

    IEnumerator PassiveCheeseRoutine(int amount, float intervalMinutes)
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalMinutes * 60f);
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResources(0, amount);
                Debug.Log($"Gained {amount} Cheese passively!");
            }
        }
    }
}