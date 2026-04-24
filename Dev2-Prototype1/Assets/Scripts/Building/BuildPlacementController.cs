using UnityEngine;

public class BuildPlacementController : MonoBehaviour
{

    [Header("----- References -----")]
    [SerializeField] Camera buildCamera;
    [SerializeField] Transform playerPos;
    [SerializeField] GameObject previewPrefab;
    [SerializeField] CurrencyManager currencyManager;

    [Header("----- Build Settings -----")]
    [SerializeField] BuildableType currentBuildType = BuildableType.Turret;
    [SerializeField] float rayCastDist = 100f;
    [SerializeField] float maxBuildDist = 8f;
    [SerializeField] float placementRadius = 1f;
    [SerializeField] float previewYOffset = 0f;

    [Header("----- Buildables -----")]
    [SerializeField] BuildableDefinition[] buildables;
    [SerializeField] int currBuildIndex = 0;

    [Header("----- Input -----")]
    [SerializeField] KeyCode togglePreviewKey = KeyCode.B;
    [SerializeField] KeyCode confirmBuildKey = KeyCode.Mouse0;
    [SerializeField] KeyCode rotatePreviewKey = KeyCode.R;
    [SerializeField] float rotateAngle = 45f;

    [Header("----- Layers -----")]
    [SerializeField] LayerMask buildAreaMask;
    [SerializeField] LayerMask placementBlockMask;

    [Header("----- Preview Colors -----")]
    [SerializeField] Color validColor = Color.blue;
    [SerializeField] Color invalidColor = Color.red;

    bool previewModeActive;
    bool currentPlacementValid;

    float currentPreviewYaw;

    Vector3 currentPlacementPos;

    GameObject previewInstance;
    Renderer[] previewRenderers;

    BuildableDefinition currBuildable;

    private void Start()
    {
        if(buildables != null && buildables.Length > 0)
        {
            currBuildIndex = Mathf.Clamp(currBuildIndex, 0, buildables.Length - 1);
            currBuildable = buildables[currBuildIndex];
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(togglePreviewKey))
        {
            currentPreviewYaw = 0;
            TogglePreviewMode();
        }

        if (!previewModeActive)
        {
            return;
        }

        HandleScrollSelection();

        if (Input.GetKeyDown(rotatePreviewKey))
        {
            RotatePreview();
        }

        UpdatePreview();

        if(Input.GetKeyDown(confirmBuildKey) && currentPlacementValid)
        {
            ConfirmBuild();
        }
    }

    void RotatePreview()
    {
        currentPreviewYaw += rotateAngle;

        if(currentPreviewYaw >= 360f)
        {
            currentPreviewYaw -= 360f;
        }
    }

    void HandleScrollSelection()
    {
        if(buildables == null  || buildables.Length == 0)
        {
            return;
        }

        float mouseScroll = Input.mouseScrollDelta.y;

        if(mouseScroll > 0f)
        {
            currBuildIndex++;

            if(currBuildIndex >= buildables.Length)
            {
                currBuildIndex = 0;
            }

            SelectBuildable(currBuildIndex);
        }
        else if(mouseScroll < 0f)
        {
            currBuildIndex--;

            if(currBuildIndex < 0)
            {
                currBuildIndex = buildables.Length - 1;
            }

            SelectBuildable(currBuildIndex);
        }
    }

    void TogglePreviewMode()
    {
        previewModeActive = !previewModeActive;

        if (previewModeActive)
        {
            SelectBuildable(currBuildIndex);
        }
        else
        {
            DestroyPreviewInstance();
        }
    }

    void SelectBuildable(int _BuildIndex)
    {
        if(buildables == null || buildables.Length == 0)
        {
            return;
        }

        currBuildIndex = _BuildIndex;
        currBuildable = buildables[currBuildIndex];

        DestroyPreviewInstance();
        CreatePreviewInstance();
    }

    void CreatePreviewInstance()
    {
        if (currBuildable == null || currBuildable.placedPreview == null)
        {
            Debug.LogWarning("[BuildPlacementController] Preview prefab is not assigned");
            previewModeActive = false;
            return;
        }

        if(previewInstance != null)
        {
            return;
        }

        previewInstance = Instantiate(currBuildable.placedPreview);
        previewRenderers = previewInstance.GetComponentsInChildren<Renderer>();

        Collider[] previewColliders = previewInstance.GetComponentsInChildren<Collider>();

        for(int i = 0; i < previewColliders.Length; i++)
        {
            previewColliders[i].enabled = false;
        }
    }

    void DestroyPreviewInstance()
    {
        currentPlacementValid = false;

        if(previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
            previewRenderers = null;
        }
    }

    void UpdatePreview()
    {
        if(previewInstance == null || currBuildable == null)
        {
            return;
        }

        Ray ray = buildCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (!Physics.Raycast(ray, out RaycastHit hit, rayCastDist, buildAreaMask, QueryTriggerInteraction.Collide))
        {
            previewInstance.SetActive(false);
            currentPlacementValid = false;
            return;
        }

        BuildArea buildArea = hit.collider.GetComponent<BuildArea>();

        if(buildArea == null)
        {
            buildArea = hit.collider.GetComponentInParent<BuildArea>();
        }

        if(buildArea == null)
        {
            previewInstance.SetActive(false);
            currentPlacementValid = false;
            currentPlacementPos = Vector3.zero;
            return;
        }

        Vector3 placementPos = hit.point;
        placementPos.y += currBuildable.previewYOffset;

        bool buildTypeAllowed = buildArea.AllowsBuildType(currBuildable.buildableType);
        bool withinBuildDist = IsWithinBuildDist(placementPos);
        bool overlapsBlockedObject = Physics.CheckSphere(placementPos, currBuildable.placementRadius, placementBlockMask, QueryTriggerInteraction.Ignore);

        // Checking cost
        bool canAfford = gamemanager.instance.currencyManager.canBuy(currBuildable.cost);

        currentPlacementValid = buildTypeAllowed && withinBuildDist && !overlapsBlockedObject && canAfford;
        currentPlacementPos = placementPos;

        previewInstance.SetActive(true);
        previewInstance.transform.position = placementPos;
        previewInstance.transform.rotation = Quaternion.Euler(0f, currentPreviewYaw, 0f);

        ApplyPreviewColor(currentPlacementValid ? validColor : invalidColor);
    }

    bool IsWithinBuildDist(Vector3 _PlacementPos)
    {
        if(playerPos == null)
        {
            return false;
        }

        return Vector3.Distance(playerPos.position, _PlacementPos) <= maxBuildDist;
    }

    void ApplyPreviewColor(Color _Tint)
    {
        if(previewRenderers == null)
        {
            return;
        }

        for(int i = 0; i < previewRenderers.Length; i++)
        {
            Material currMat = previewRenderers[i].material;

            if (currMat.HasProperty("_Color"))
            {
                currMat.color = _Tint;
            }
        }
    }

    void ConfirmBuild()
    {
        if(currBuildable == null || currBuildable.placedPreview == null)
        {
            Debug.LogWarning("[BuildPlacementController] TurretManager is not assigned", this);
            return;
        }

        if(currencyManager == null)
        {
            Debug.LogWarning("[BuildPlacementController] CurrencyManager is not assigned", this);
            return;
        }

        if (!currencyManager.SpendCurrency(currBuildable.cost))
        {
            return;
        }

        Quaternion buildRotation = Quaternion.Euler(0f, currentPreviewYaw, 0f);
        GameObject builtObject = Instantiate(currBuildable.placedPrefab, currentPlacementPos, buildRotation);

        if(builtObject == null)
        {
            Debug.LogWarning("[BuildPlacementController] Build failed", this);
            gamemanager.instance.currencyManager.AddCurrency(currBuildable.cost); // Refund currency if failed
            return;
        }

        PlacedBuildable placedBuildable = builtObject.GetComponent<PlacedBuildable>();

        if(placedBuildable != null)
        {
            placedBuildable.Init(currBuildable);
        }

    }

}
