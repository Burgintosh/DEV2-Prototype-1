using UnityEngine;


// For the individual slots of the programmatically created hotbar.
// Does the heavy lifting of creating the actual prefabs and centering them.
// Meant to be used in tangem with BuildUIHotbar.cs to create and organize multiple of these.
public class BuildUISlot : MonoBehaviour
{
    [SerializeField] private Transform modelContainer;
    [SerializeField] private GameObject selectedBorder;
    [SerializeField] private float miniatureScale = 15f;

    private GameObject currentModel;
    BuildableDefinition buildable;

    public void Setup(BuildableDefinition _buildData)
    {
        buildable = _buildData;
        if (currentModel != null)
            Destroy(currentModel);

        if (buildable.placedPrefab != null && modelContainer != null)
        {
            currentModel = Instantiate(buildable.placedPrefab, modelContainer);

            SetUILayerRecursively(currentModel); // Needed for rendering correctly

            currentModel.transform.localScale = Vector3.one * miniatureScale;
            currentModel.transform.localPosition = Vector3.zero;
            //currentModel.transform.localRotation = Quaternion.Euler(15f, 215f, 0f);


            DisableComponents(currentModel);
            // CenterModel(currentModel, modelContainer);
        }
    }

    // Not working for the laser trap so disabling for now. Gonna come back to it later and try to make it better.
    // I think the issue might have to do with the particle system on the hit effect but it's disabled
    private void CenterModel(GameObject model, Transform container)
    {
        // Cool stuff, takes all the meshes in the prefab and ensures the whole thing is within the ui box.
        // Obviously doesn't work if the tower is too big. but works for now.
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return;

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Vector3 offset = container.position - bounds.center;

        model.transform.position += offset;

        
    }

    public void SetSelected(bool selected)
    {
        selectedBorder.SetActive(selected);
    }
    public BuildableDefinition GetBuildable()
    {
        return buildable;
    }

    private void SetUILayerRecursively(GameObject obj)
    {
        obj.layer = LayerMask.NameToLayer("UI");
        foreach (Transform child in obj.transform)
            SetUILayerRecursively(child.gameObject);
    }

    private void DisableComponents(GameObject model)
    {
        MonoBehaviour[] scripts = model.GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
                script.enabled = false;

        // Removes colliders from prefabs in UI so they don't break anything
        // I assume this is necessary, google said so but I didn't test without it
        Collider[] colliders = model.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders) col.enabled = false;

        LineRenderer[] lines = model.GetComponentsInChildren<LineRenderer>(); // turning off the line renderer for the bounds used below
        foreach (LineRenderer line in lines) line.enabled = false;
    }
}
