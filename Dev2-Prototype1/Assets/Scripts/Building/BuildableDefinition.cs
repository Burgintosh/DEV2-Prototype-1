using UnityEngine;

[System.Serializable]
// Should be a scriptable object to be more modular.
// Doing so now will clear all the trap data in the BuildPlacementController script's buildables list.
// Make sure to make those up if we do that.
public class BuildableDefinition 
{
    public string buildName;
    public BuildableType buildableType;
    public GameObject placedPrefab;
    public GameObject placedPreview;
    public int cost;
    public int refundAmount = 100;
    public float placementRadius = 1f;
    public bool useExtraPlacementCheck;
    public Vector3 extraPlacementOffset = Vector3.zero;
    public float extraPlacementCheckRad = 0.5f;
    public float previewYOffset = 0f;
    public BuildPlacementMode placementMode = BuildPlacementMode.Flat;
    public Vector3 surfaceRotOffset;
    public BuildSpinAxis surfaceSpinAxis = BuildSpinAxis.Forward;
}
