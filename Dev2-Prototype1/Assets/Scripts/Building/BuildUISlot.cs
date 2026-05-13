using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BuildUISlot : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject selectedBorder;

    BuildableDefinition buildable;

    public void Setup(BuildableDefinition _buildData)
    {
        buildable = _buildData;
     
    }
    public void SetSelected(bool selected)
    {
        selectedBorder.SetActive(selected);
    }
}
