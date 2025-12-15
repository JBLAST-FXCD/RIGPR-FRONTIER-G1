using UnityEngine;
using System.Collections;

/// <summary>
/// Manages all upgrades for commercial buildings specifically.
/// Handles profits from sales, offer events, and cheese bonuses.
/// </summary>
public class CommercialUpgradeHandler : BuildingUpgradeHandler
{
    [Header("Base Stats (Editable In-Script)")]
    public float profitMultiplier = 1.0f;
    public float saleFrequencyMultiplier = 1.0f;
    public float chanceForRandomCheese = 0f;
    public int randomCheeseAmount = 0;
    public bool halfPriceConstruction = false;

    [Header("Configurables - Stats")]
    [SerializeField] private float profitBonusSmall = 0.10f;
    [SerializeField] private float freqBonusSmall = 0.10f;

    [Header("Configurables - Cheese Bonuses")]
    [SerializeField] private float refundChance = 0.10f;
    [SerializeField] private int smallCheeseReward = 5;
    [SerializeField] private int decentCheeseReward = 15;

    [Header("Configurables - Limited-Time Offer Event")]
    [SerializeField] private float eventCooldownMins = 15f;
    [SerializeField] private float eventDurationMins = 2f;
    [SerializeField] private float eventProfitMult = 2.0f;

    protected override void ApplyUpgradeEffect(UpgradeDefinition upgrade)
    {
        switch (upgrade.upgradeID)
        {
            case "COM_2_1":
                profitMultiplier += profitBonusSmall;
                break;
            case "COM_2_2":
                saleFrequencyMultiplier += freqBonusSmall;
                break;
            case "COM_2_3":
                chanceForRandomCheese = refundChance;
                randomCheeseAmount = smallCheeseReward;
                break;
            case "COM_3":
                StartCoroutine(LimitedTimeOfferRoutine());
                break;
            case "COM_4_1":
                profitMultiplier += profitBonusSmall;
                break;
            case "COM_4_2":
                saleFrequencyMultiplier += freqBonusSmall;
                break;
            case "COM_4_3":
                randomCheeseAmount = decentCheeseReward;
                break;
            case "COM_5":
                halfPriceConstruction = true;
                break;
            case "COM_6":
                BuildingUnlockManager.Instance.UnlockBuilding("DockYard");
                break;
        }
    }

    IEnumerator LimitedTimeOfferRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(eventCooldownMins * 60f);
            Debug.Log("Limited time offer has begun!");
            float oldMult = profitMultiplier;
            profitMultiplier *= eventProfitMult;

            yield return new WaitForSeconds(eventDurationMins * 60f);
            profitMultiplier = oldMult;
            Debug.Log("Limited time offer has ended.");
        }
    }
}
