using UnityEngine;

public class PlacedBuildable : MonoBehaviour
{
    [SerializeField] BuildableType buildableType;
    [SerializeField] int refundAmount;

    public void Init(BuildableDefinition _BuildDef)
    {
        buildableType = _BuildDef.buildableType;
        refundAmount = _BuildDef.refundAmount;
    }

    public BuildableType GetBuildableType()
    {
        return buildableType;
    }

    public int GetRefundAmount()
    {
        return refundAmount;
    }
}
