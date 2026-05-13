using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class BuildUIHotbar : MonoBehaviour
{
    [SerializeField] BuildUISlot slotPrefab;
    [SerializeField] Transform slotParent;

    [Header("----- Info Text -----")]
    [SerializeField] TMP_Text towerNameText;
    [SerializeField] TMP_Text towerCostText;

    private List<BuildUISlot> slots = new List<BuildUISlot>();

    public void Initialize(BuildableDefinition[] buildables)
    {
        for (int i = 0; i < buildables.Length; i++)
        {
            BuildUISlot slot = Instantiate(slotPrefab, slotParent);
            slot.Setup(buildables[i]);
            slots.Add(slot);
        }
    }
    public void SetSelectedIndex(int index)
    {
        for (int i = 0; i < slots.Count; i++) // sets non-active slot borders to off
        {
            bool selected = (i == index);

            slots[i].SetSelected(selected);

            if (selected)
                UpdateInfoText(slots[i].GetBuildable());
        }
    }

    void UpdateInfoText(BuildableDefinition buildData)
    {
        if (buildData == null) return;

        if (towerNameText != null)
            towerNameText.text = buildData.buildName;

        if (towerCostText != null)
            towerCostText.text = "$" + buildData.cost.ToString();
    }
}
