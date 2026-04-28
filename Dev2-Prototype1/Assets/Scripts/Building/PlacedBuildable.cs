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

    public void Sell(CurrencyManager _CurrencyManager)
    {
        if(_CurrencyManager == null)
        {
            Debug.LogWarning("[PlacedBuildable] CurrencyManager is null, cannot sell buildable");
            return;
        }

        _CurrencyManager.AddCurrency(refundAmount);
        Destroy(gameObject);
    }

}
