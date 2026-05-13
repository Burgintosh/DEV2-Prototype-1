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
            slots[i].SetSelected(i == index);
        }
    }

    void UpdateInfoText(int index)
    {
        if (currBuildables == null) return;

        if (index < 0 || index >= currBuildables.Length) return;

        BuildableDefinition buildable = currBuildables[index];

        if (towerNameText != null)
        {
            towerNameText.text =
                buildable.buildName;
        }

        if (towerCostText != null)
        {
            towerCostText.text =
                "$" + buildable.cost.ToString();
        }
    }
}
