using UnityEngine;

[CreateAssetMenu]
// ScriptableObject version of BuildableDefinition.Just making this to save some turret info we're removing from beta.
public class BuildableData : ScriptableObject
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
    public Vector3 surfaceRotOffset;
    public BuildSpinAxis surfaceSpinAxis = BuildSpinAxis.Forward;
}
