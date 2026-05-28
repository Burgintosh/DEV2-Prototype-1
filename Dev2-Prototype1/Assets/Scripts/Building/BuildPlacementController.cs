using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildPlacementController : MonoBehaviour
{

    [Header("----- References -----")]
    [SerializeField] Camera buildCamera;
    [SerializeField] Transform playerPos;
    [SerializeField] GameObject previewPrefab;
    [SerializeField] CurrencyManager currencyManager;

    [Header("----- Build Settings -----")]
    [SerializeField] float rayCastDist = 100f;
    [SerializeField] float maxBuildDist = 8f;

    [Header("----- Sell Settings -----")]
    [SerializeField] float sellDist = 10f;
    private GameObject sellPromptUI;

    [Header("----- Buildables -----")]
    [SerializeField] BuildableDefinition[] buildables;
    [SerializeField] int currBuildIndex = 0;
    BuildUIHotbar hotbarUI;

    [Header("----- Input -----")]
    [SerializeField] KeyCode togglePreviewKey = KeyCode.B;
    [SerializeField] KeyCode confirmBuildKey = KeyCode.Mouse0;
    [SerializeField] KeyCode rotatePreviewKey = KeyCode.R;
    [SerializeField] KeyCode sellBuildKey = KeyCode.E;
    [SerializeField] float rotateAngle = 45f;

    [Header("----- Layers -----")]
    [SerializeField] LayerMask buildAreaMask;
    [SerializeField] LayerMask placementBlockMask;

    [Header("----- Preview Colors -----")]
    [SerializeField] Color validColor = Color.blue;
    [SerializeField] Color invalidColor = Color.red;

    [Header("----- Build Feedback SFX -----")]
    [SerializeField] AudioSource buildFeedbackAudioSource;
    [SerializeField] AudioClip placeBuildableSFX;
    [SerializeField] AudioClip sellBuildableSFX;
    [SerializeField] AudioClip rotBuildableSFX;
    [SerializeField] AudioClip failedBuildableSFX;
    [SerializeField] float buildFeedbackVol = 1f;
    [SerializeField] SoundCategory buildFeedbackSoundCategory = SoundCategory.Trap;

    bool previewModeActive;
    bool currentPlacementValid;
    bool currentCanRotPreview;

    float currentPreviewYaw;

    Vector3 currentPlacementPos;
    Vector3 currentSurfaceNormal;
    Quaternion currentPlacementRot;

    GameObject previewInstance;
    Renderer[] previewRenderers;
    BuildableRangeDisplay previewRangeDisplay;

    BuildableDefinition currBuildable;

    private void Start()
    {
        if (buildables != null && buildables.Length > 0)
        {
            currBuildIndex = Mathf.Clamp(currBuildIndex, 0, buildables.Length - 1);
            currBuildable = buildables[currBuildIndex];
        }

        if (hotbarUI == null)
            //hotbarUI = FindFirstObjectByType<BuildUIHotbar>();

        if (hotbarUI != null)
        {
            //hotbarUI.Initialize(buildables);
            //hotbarUI.SetSelectedIndex(currBuildIndex);
        }
        else
        {
            //Debug.LogWarning("BuildUIHotbar is missing from the scene!");
        }

        if (sellPromptUI  == null)
        {
            sellPromptUI = gamemanager.instance.sellPromptUI;
        }
        SetCurrencyManager();
    }

    private void Update()
    {
        if (Input.GetKeyDown(sellBuildKey))
        {
            TrySellBuildable();
        }

        if (Input.GetKeyDown(togglePreviewKey))
        {
            currentPreviewYaw = 0;
            TogglePreviewMode();
        }

        UpdateSellPrompt();

        if (!previewModeActive)
        {
            return;
        }

        if (IsPreviewMode()) // Added check to only allow scrolling when in preview mode, allows scrolling between builds & guns in their respective modes
            HandleScrollSelection();

        UpdatePreview();

        if (Input.GetKeyDown(rotatePreviewKey))
        {
            RotatePreview();
        }

        if(Input.GetKeyDown(confirmBuildKey))
        {
            if (currentPlacementValid)
            {
                ConfirmBuild();
            }
            else
            {
                PlayBuildFeedbackSFX(failedBuildableSFX);
            }
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void RotatePreview()
    {
        if (!currentCanRotPreview)
        {
            PlayBuildFeedbackSFX(failedBuildableSFX);
            return;
        }

        currentPreviewYaw += rotateAngle;

        if(currentPreviewYaw >= 360f)
        {
            currentPreviewYaw -= 360f;
        }

        currentPlacementRot = GetPlacementRot(currentSurfaceNormal);

        if(previewInstance != null && previewInstance.activeSelf)
        {
            previewInstance.transform.rotation = currentPlacementRot;
        }

        PlayBuildFeedbackSFX(rotBuildableSFX);
    }

    Quaternion GetPlacementRot(Vector3 _SurfaceNormal)
    {
        if(currBuildable == null)
        {
            return Quaternion.identity;
        }

        if(currBuildable.placementMode == BuildPlacementMode.Flat)
        {
            return Quaternion.Euler(0f, currentPreviewYaw, 0f);
        }

        Vector3 refUp = Mathf.Abs(Vector3.Dot(_SurfaceNormal, Vector3.up)) > 0.98f ? Vector3.forward : Vector3.up;
        Quaternion baseRot = Quaternion.LookRotation(_SurfaceNormal, refUp);
        Quaternion offsetRot = Quaternion.Euler(currBuildable.surfaceRotOffset);
        Quaternion alignedRot = baseRot * offsetRot;

        Vector3 localSpinAxis = Vector3.forward;

        switch (currBuildable.surfaceSpinAxis)
        {
            case BuildSpinAxis.Up:
                localSpinAxis = Vector3.up;
                break;

            case BuildSpinAxis.Right:
                localSpinAxis = Vector3.right;
                break;

            case BuildSpinAxis.Forward:
            default:
                localSpinAxis = Vector3.forward;
                break;

        }

        Vector3 worldSpinAxis = alignedRot * localSpinAxis;
        Quaternion spinRot = Quaternion.AngleAxis(currentPreviewYaw, worldSpinAxis);

        return spinRot * alignedRot;
    }

    void HandleScrollSelection()
    {
        if (!IsPreviewMode()) return;

        if (buildables == null  || buildables.Length == 0)
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

        if (hotbarUI != null)
        {
            hotbarUI.gameObject.SetActive(previewModeActive);
        }
        gamemanager.instance.playerScript.weaponHolder.SetActive(!previewModeActive);
        gamemanager.instance.playerScript.blueprintHolder.SetActive(previewModeActive);
    }

    void SelectBuildable(int _BuildIndex)
    {
        if(buildables == null || buildables.Length == 0)
        {
            return;
        }

        currBuildIndex = _BuildIndex;
        currBuildable = buildables[currBuildIndex];

        currentPreviewYaw = 0;

        DestroyPreviewInstance();
        CreatePreviewInstance();

        if (hotbarUI  != null)
        {
            hotbarUI.SetSelectedIndex(currBuildIndex);
            hotbarUI.UpdateInfoText(currBuildable);
        }
    }

    void CreatePreviewInstance()
    {
        if (currBuildable == null || currBuildable.placedPreview == null)
        {
            //Debug.LogWarning("[BuildPlacementController] Preview prefab is not assigned");
            previewModeActive = false;
            return;
        }

        if(previewInstance != null)
        {
            return;
        }

        previewInstance = Instantiate(currBuildable.placedPreview);
        previewRenderers = previewInstance.GetComponentsInChildren<Renderer>(true);

        Collider[] previewColliders = previewInstance.GetComponentsInChildren<Collider>();

        for(int i = 0; i < previewColliders.Length; i++)
        {
            previewColliders[i].enabled = false;
        }

        previewRangeDisplay = previewInstance.GetComponentInChildren<BuildableRangeDisplay>(true);
        
        //if(previewRangeDisplay == null)
        //{
        //    Debug.LogWarning("BuildPlacementController found no BuildableRangeDisplay on preview " + previewInstance.name, previewInstance);
        //}
        //else
        //{
        //    Debug.Log("BuildPlacementController found BuildableRangeDisplay on preview " + previewInstance.name, previewRangeDisplay);
        //}

        if (previewRangeDisplay != null)
        {
            previewRangeDisplay.ShowRange();
        }

    }

    void DestroyPreviewInstance()
    {
        currentPlacementValid = false;
        previewRangeDisplay = null;

        if(previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
            previewRenderers = null;
        }
    }

    void UpdatePreview()
    {
        currentCanRotPreview = false;
        currentPlacementValid = false;

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
        Vector3 surfaceNormal = buildArea.GetSurfaceNormal().normalized;

        Vector3 toCam = (buildCamera.transform.position - buildArea.transform.position).normalized;

        if(Vector3.Dot(surfaceNormal, toCam) > 0)
        {
            surfaceNormal = -surfaceNormal;
        }

        placementPos += surfaceNormal * currBuildable.previewYOffset;

        bool buildTypeAllowed = buildArea.AllowsBuildType(currBuildable.buildableType);
        bool withinBuildDist = IsWithinBuildDist(placementPos);
        bool overlapsBlockedObject = IsPlacementBlocked(placementPos, hit.collider, buildArea);

        // Checking cost
        bool canAfford = gamemanager.instance.currencyManager.canBuy(currBuildable.cost);

        currentCanRotPreview = buildTypeAllowed && withinBuildDist;

        currentPlacementValid = buildTypeAllowed && withinBuildDist && !overlapsBlockedObject && canAfford;
        currentPlacementPos = placementPos;
        currentSurfaceNormal = surfaceNormal;
        currentPlacementRot = GetPlacementRot(surfaceNormal);

        previewInstance.SetActive(true);
        previewInstance.transform.position = placementPos;
        previewInstance.transform.rotation = currentPlacementRot;

        if(previewRangeDisplay != null)
        {
            previewRangeDisplay.RefreshRangeVisual();
        }

        ApplyPreviewColor(currentPlacementValid ? validColor : invalidColor);
    }

    private void UpdateSellPrompt()
    {
        if (sellPromptUI == null) return;

        if(!IsPreviewMode())
        {
            if (sellPromptUI.activeSelf)
                sellPromptUI.SetActive(false);
            return;
        }

        PlacedBuildable targetedBuildable = GetTargetedBuildable();
        bool canSell = (targetedBuildable != null);

        if (sellPromptUI.activeSelf != canSell)
        {
            sellPromptUI.SetActive(canSell);
        }

    }
    private PlacedBuildable GetTargetedBuildable() // Helper function to avoid reusing the code in TrySellBuildable in new UpdateSellPrompt method
    {
        if (buildCamera == null) return null;

        Ray ray = buildCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (!Physics.Raycast(ray, out RaycastHit hit, sellDist, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            return null;
        }

        //Debug.Log("Sell ray hit: " + hit.collider.name + " | Root: " + hit.collider.transform.name + " | Is Trigger: " + hit.collider.isTrigger + " | Layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer), hit.collider.gameObject);

        PlacedBuildable placedBuildable = hit.collider.GetComponent<PlacedBuildable>();

        //Debug.Log("Selling buildables: " + placedBuildable.name, placedBuildable.gameObject);

        if (placedBuildable == null)
            placedBuildable = hit.collider.GetComponentInParent<PlacedBuildable>();

        if (placedBuildable == null)
        {
            //Debug.Log("No PlaceBuildable found on hit object or parents.", hit.collider.gameObject);
        }

        return placedBuildable;
    }

    bool IsWithinBuildDist(Vector3 _PlacementPos)
    {
        if(playerPos == null)
        {
            return false;
        }

        return Vector3.Distance(playerPos.position, _PlacementPos) <= maxBuildDist;
    }

    bool IsPlacementBlocked(Vector3 _PlacementPos, Collider _HitCollider, BuildArea _CurrBuildArea)
    {
        Collider[] allHits = Physics.OverlapSphere(_PlacementPos, currBuildable.placementRadius, placementBlockMask, QueryTriggerInteraction.Ignore);

        for(int i = 0; i < allHits.Length; i++)
        {
            Collider currHit = allHits[i];

            if(currHit == null)
            {
                continue;
            }

            if(currHit == _HitCollider)
            {
                continue;
            }

            BuildArea hitBuildArea = currHit.GetComponentInParent<BuildArea>();

            if(hitBuildArea != null && hitBuildArea == _CurrBuildArea)
            {
                continue;
            }

            if (previewInstance != null && currHit.transform.IsChildOf(previewInstance.transform))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    void ApplyPreviewColor(Color _Tint)
    {
        if(previewRenderers == null)
        {
            return;
        }

        for(int i = 0; i < previewRenderers.Length; i++)
        {
            if(previewRangeDisplay != null && previewRangeDisplay.IsRangeRend(previewRenderers[i]))
            {
                continue;
            }

            Material currMat = previewRenderers[i].material;

            if (currMat.HasProperty("_Color"))
            {
                currMat.color = _Tint;
            }
        }
    }

    void ConfirmBuild()
    {
        if(currBuildable == null || currBuildable.placedPrefab == null)
        {
            //Debug.LogWarning("[BuildPlacementController] Placed prefab is not assigned", this);
            return;
        }

        if(currencyManager == null)
        {
            //Debug.LogWarning("[BuildPlacementController] CurrencyManager is not assigned", this);
            return;
        }

        if (!currencyManager.SpendCurrency(currBuildable.cost))
        {
            return;
        }

        Quaternion buildRotation = currentPlacementRot;
        GameObject builtObject = Instantiate(currBuildable.placedPrefab, currentPlacementPos, buildRotation);

        if(builtObject == null)
        {
            //Debug.LogWarning("[BuildPlacementController] Build failed", this);
            gamemanager.instance.currencyManager.AddCurrency(currBuildable.cost); // Refund currency if failed
            return;
        }

        PlacedBuildable placedBuildable = builtObject.GetComponent<PlacedBuildable>();

        if(placedBuildable != null)
        {
            placedBuildable.Init(currBuildable);
        }

        PlayBuildFeedbackSFX(placeBuildableSFX);
    }

    void TrySellBuildable()
    {
        if(buildCamera == null)
        {
            //Debug.LogWarning("[BuildPlacementController] Build camera is not assigned.", this);
            return;
        }

        if(currencyManager == null)
        {
            //Debug.LogWarning("[BuildPlacementController] CurrencyManager is not assigned", this);
            return;
        }

        //PlacedBuildable placedBuildable = hit.collider.GetComponent<PlacedBuildable>();

        //placedBuildable.Sell(currencyManager);

        PlacedBuildable placedBuildable = GetTargetedBuildable(); // Replaced the above code with a helper function

        if (placedBuildable != null)
        {
            placedBuildable.Sell(currencyManager);

            PlayBuildFeedbackSFX(sellBuildableSFX);
        }
            
    }

    void PlayBuildFeedbackSFX(AudioClip _AudioClip)
    {
        if(_AudioClip == null)
        {
            //Debug.LogWarning("[BuildPlacementController] Attempted to play build SFX no audioclip was assigned", this);
            return;
        }

        if(buildFeedbackAudioSource == null)
        {
            //Debug.LogWarning("[BuildPlacementController] Attempted toplay build feedback SFX, but AudioSource wasn't assigned");
            return;
        }

        if(SoundManager.Instance == null)
        {
            //Debug.LogWarning("[BuildPlacementController] SoundManager instance missing", this);
            return;
        }

        SoundManager.Instance.PlayWithRandomPitch(buildFeedbackAudioSource, _AudioClip, buildFeedbackVol, buildFeedbackSoundCategory, true);

    }

    public bool IsPreviewMode()
    {
        return previewModeActive;
    }
    private void SetCurrencyManager()
    {
        if(currencyManager == null)
        {
            currencyManager = gamemanager.instance.currencyManager;
        }
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (previewModeActive)
        {
            TogglePreviewMode();
        }
        SetCurrencyManager();
        if( hotbarUI == null)
        {
            hotbarUI = FindFirstObjectByType<BuildUIHotbar>();
            if(hotbarUI != null)
            {
                hotbarUI.Initialize(buildables);
                hotbarUI.SetSelectedIndex(currBuildIndex);
            }
        }
        if(sellPromptUI == null)
        {
            sellPromptUI = gamemanager.instance.sellPromptUI;
            if(sellPromptUI != null)
            {
                sellPromptUI.SetActive(false);
            }
        }
    }
}
