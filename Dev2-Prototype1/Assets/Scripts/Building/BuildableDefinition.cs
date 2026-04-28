using UnityEngine;

[System.Serializable]
public class BuildableDefinition
{
    public string buildName;
    public BuildableType buildableType;
    public GameObject placedPrefab;
    public GameObject placedPreview;
    public int cost;
    public int refundAmount = 100;
    public float placementRadius = 1f;
    public float previewYOffset = 0f;
    public BuildPlacementMode placementMode = BuildPlacementMode.Flat;
}
