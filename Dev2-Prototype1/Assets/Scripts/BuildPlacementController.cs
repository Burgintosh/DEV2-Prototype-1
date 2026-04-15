using UnityEngine;

public class BuildPlacementController : MonoBehaviour
{

    [Header("----- References -----")]
    [SerializeField] Camera buildCamera;
    [SerializeField] Transform playerPos;
    [SerializeField] TurretManager turretManager;
    [SerializeField] GameObject previewPrefab;

    [Header("----- Build Settings -----")]
    [SerializeField] BuildableType currentBuildType = BuildableType.Turret;
    [SerializeField] float rayCastDist = 100f;
    [SerializeField] float maxBuildDist = 8f;
    [SerializeField] float placementRadius = 1f;
    [SerializeField] float previewYOffset = 0f;

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

    void TogglePreviewMode()
    {
        previewModeActive = !previewModeActive;

        if (previewModeActive)
        {
            CreatePreviewInstance();
        }
        else
        {
            DestroyPreviewInstance();
        }
    }

    void CreatePreviewInstance()
    {
        if(previewPrefab == null)
        {
            Debug.LogWarning("[BuildPlacementController] Preview prefab is not assigned");
            previewModeActive = false;
            return;
        }

        if(previewInstance != null)
        {
            return;
        }

        previewInstance = Instantiate(previewPrefab);
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
        if(previewInstance == null)
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
        placementPos.y += previewYOffset;

        bool buildTypeAllowed = buildArea.AllowsBuildType(currentBuildType);
        bool withinBuildDist = IsWithinBuildDist(placementPos);
        bool overlapsBlockedObject = Physics.CheckSphere(placementPos, placementRadius, placementBlockMask, QueryTriggerInteraction.Ignore);

        // Checking cost
        int turretCost = turretManager.GetTurretCost();
        bool canAfford = gamemanager.instance.currencyManager.canBuy(turretCost);

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
        if(turretManager == null)
        {
            Debug.LogWarning("[BuildPlacementController] TurretManager is not assigned", this);
            return;
        }

        int turretCost = turretManager.GetTurretCost();
        gamemanager.instance.currencyManager.SpendCurrency(turretCost);

        Quaternion buildRotation = Quaternion.Euler(0f, currentPreviewYaw, 0f);
        PooledTurret builtTurret = turretManager.BuildTurret(currentPlacementPos, buildRotation);

        if(builtTurret == null)
        {
            Debug.LogWarning("[BuildPlacementController] Build failed", this);
            gamemanager.instance.currencyManager.AddCurrency(turretCost); // Refund currency if failed
        }
    }

}
