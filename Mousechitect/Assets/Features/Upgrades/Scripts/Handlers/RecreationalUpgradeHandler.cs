using UnityEngine;
using System.Collections;

/// <summary>
/// Manages all upgrades for recreational buildings specifically.
/// Handles morale bonuses, events & deductions as well as passive cheese generation.
/// </summary>
public class RecreationUpgradeHandler : BuildingUpgradeHandler
{
    [Header("Base Stats (Editable In-Script)")]
    public float moraleIncreaseMultiplier = 1.0f;
    public float moraleDecayMultiplier = 1.0f;
    public bool halfPriceConstruction = false;
    public bool grandOpeningUnlocked = false;
    public bool isMoraleFrozen = false;

    [Header("Configurables - Morale Multipliers")]
    [SerializeField] private float funTimesBonus = 0.01f;
    [SerializeField] private float dayToRememberBonus = 0.03f;
    [SerializeField] private float brightSideDecayReduction = 0.10f;
    [SerializeField] private float brighterSideDecayReduction = 0.15f;

    [Header("Configurables - Passive Cheese")]
    [SerializeField] private int smallGroupCheese = 5;
    [SerializeField] private float vegasIntervalMinutes = 15f;
    [SerializeField] private float taxWriteOffIntervalMinutes = 10f;

    [Header("Configuration - Special Ability")]
    [SerializeField] private float grandOpeningDurationMinutes = 3f;

    private Coroutine passiveCheeseRoutine;

    protected override void ApplyUpgradeEffect(UpgradeDefinition upgrade)
    {
        switch (upgrade.upgradeID)
        {
            case "REC_2_1":
                moraleIncreaseMultiplier += funTimesBonus;
                break;
            case "REC_2_2":
                if (passiveCheeseRoutine != null) StopCoroutine(passiveCheeseRoutine);
                passiveCheeseRoutine = StartCoroutine(GroupBonusCheeseRoutine(smallGroupCheese, vegasIntervalMinutes));
                break;
            case "REC_2_3":
                moraleDecayMultiplier -= brightSideDecayReduction;
                break;
            case "REC_3":
                grandOpeningUnlocked = true;
                break;
            case "REC_4_1":
                moraleIncreaseMultiplier += dayToRememberBonus;
                break;
            case "REC_4_2":
                if (passiveCheeseRoutine != null) StopCoroutine(passiveCheeseRoutine);
                passiveCheeseRoutine = StartCoroutine(GroupBonusCheeseRoutine(smallGroupCheese, taxWriteOffIntervalMinutes));
                break;
            case "REC_4_3":
                moraleDecayMultiplier -= brighterSideDecayReduction;
                break;
            case "REC_5":
                halfPriceConstruction = true;
                break;
        }
    }

    // Grid manager script will need to be linked below so the bonus cheese reward scales with the number of recreational buildings in a given area.

    IEnumerator GroupBonusCheeseRoutine(int amount, float intervalMinutes)
    {
        while (true)
        {
            yield return new WaitForSeconds(intervalMinutes * 60f);
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddResources(0, amount);
            }
        }
    }


    // This function will need to be called from the building system/placement script whenever a new Recreation-Type building is built.

    public void TriggerGrandOpening()
    {
        if (grandOpeningUnlocked && !isMoraleFrozen)
        {
            StartCoroutine(GrandOpeningRoutine());
        }
    }

    IEnumerator GrandOpeningRoutine()
    {
        isMoraleFrozen = true;
        yield return new WaitForSeconds(grandOpeningDurationMinutes * 60f);
        isMoraleFrozen = false;
    }
}
