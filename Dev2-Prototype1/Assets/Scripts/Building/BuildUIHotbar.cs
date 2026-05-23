using UnityEngine;
using System.Collections.Generic;
using TMPro;

// Programmatically gathers all buildables from the player's BuildPlacementController and puts them into a ui hotbar.
// Mainly used for organizing the BuildSlot prefabs that contain BuildUISlot.cs.
public class BuildUIHotbar : MonoBehaviour
{
    [SerializeField] BuildUISlot slotPrefab;
    [SerializeField] Transform slotParent;

    [Header("----- Info Text -----")]
    [SerializeField] TMP_Text towerNameText;
    [SerializeField] TMP_Text towerCostText;

    //private List<BuildUISlot> slots = new List<BuildUISlot>();
    [SerializeField] private List<BuildUISlot> slots;
    private BuildableDefinition currentBuildable;

    private void OnEnable()
    {
        if (gamemanager.instance != null && gamemanager.instance.currencyManager != null)
            gamemanager.instance.currencyManager.OnCurrencyChanged += HandleCurrencyChanged;
    }

    private void OnDisable()
    {
        if (gamemanager.instance != null && gamemanager.instance.currencyManager != null)
            gamemanager.instance.currencyManager.OnCurrencyChanged -= HandleCurrencyChanged;
    }
    private void HandleCurrencyChanged(int amount)
    {
        UpdateInfoText(currentBuildable);
    }
    public void Initialize(BuildableDefinition[] buildables)
    {
        //for (int i = 0; i < buildables.Length; i++)
        //{
        //    BuildUISlot slot = Instantiate(slotPrefab, slotParent);
        //    slot.Setup(buildables[i]);
        //    slots.Add(slot);
        //}

        gameObject.SetActive(false);
    }
    public void SetSelectedIndex(int index)
    {
        for (int i = 0; i < slots.Count; i++) // sets non-active slot borders to off. Might be overkill to do each time? Come back here if performance is bad
        {
            bool selected = (i == index);

            slots[i].SetSelected(selected);

            if (selected)
                UpdateInfoText(slots[i].GetBuildable());
        }
    }

    public void UpdateInfoText(BuildableDefinition buildData)
    {
        if (buildData == null) return;
        currentBuildable = buildData;

        if (towerNameText != null)
            towerNameText.text = buildData.buildName;

        if (towerCostText != null)
            towerCostText.text = "$" + buildData.cost.ToString();

        // Turns cost red if the player cannot afford the trap and green if they can
        if (gamemanager.instance != null && gamemanager.instance.currencyManager != null)
        {
            bool canAfford = gamemanager.instance.currencyManager.canBuy(buildData.cost);
            towerCostText.color = canAfford ? Color.green : Color.red;
        }
    }
}
