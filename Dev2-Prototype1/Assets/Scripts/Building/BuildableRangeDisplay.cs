using UnityEngine;
using UnityEngine.Rendering;

public enum BuildableRangeSrc
{
    ManualRadius,
    SphereCollider,
    BoxCollider
}

public class BuildableRangeDisplay : MonoBehaviour
{
    [Header("----- Range Src -----")]
    [SerializeField] BuildableRangeSrc rangeSrc = BuildableRangeSrc.ManualRadius;
    [SerializeField] float manualRadius = 3f;
    [SerializeField] Collider rangeCollider;

    [Header("----- Settings -----")]
    [SerializeField] Color rangeColor = new Color(0f, .7f, 1f, 0.25f);
    [SerializeField] Material rangeMat;
    [SerializeField] int rangeCircles = 64;
    [SerializeField] float visualYOffset = 0.05f;

    [SerializeField] Collider[] selectedColliders;

    [Header("----- Debug -----")]
    [SerializeField] bool showInsantlyForTesting;

    GameObject rangeVisuals;
    MeshRenderer rangeRend;
    MeshFilter rangeMeshFilter;
    Mesh shownMesh;

    float currMeshRadius = -1f;
    int currMeshSegment = -1;

    bool isShowing;

    private void Awake()
    {
        CreateRangeCircleObject();

        if (showInsantlyForTesting)
        {
            ShowRange();
        }
        else
        {
            HideRange();
        }
    }

    private void OnDisable()
    {
        HideRange();
    }

    private void OnDestroy()
    {
        DestroyShownMesh();
    }

    public void ShowRange()
    {
        isShowing = true;

        CreateRangeCircleObject();
        RefreshRangeVisual();

        if(rangeVisuals != null)
        {
            rangeVisuals.SetActive(true);
        }
    }

    public void HideRange()
    {
        isShowing = false;

        if(rangeVisuals != null)
        {
            rangeVisuals.SetActive(false);
        }
    }

    public void RefreshRangeVisual()
    {
        if(rangeVisuals == null)
        {
            return;
        }

        if(rangeMeshFilter == null)
        {
            return;
        }

        float radiusToShow = GetRangeRadius();

        if (radiusToShow <= 0f)
        {
            rangeVisuals.SetActive(false);
            return;
        }

        RebuildMeshIfNeed(radiusToShow);

        rangeVisuals.transform.position = GetRangeCenter() + GetRangeUpDir() * visualYOffset;
        rangeVisuals.transform.rotation = GetRangeRot();

        ApplyRangeCircleColor();

        if (isShowing)
        {
            rangeVisuals.SetActive(true);
        }
    }

    void CreateRangeCircleObject()
    {
        if(rangeVisuals != null)
        {
            return;
        }

        rangeVisuals = new GameObject("RangeVisuals");
        rangeVisuals.transform.SetParent(transform, true);

        rangeMeshFilter = rangeVisuals.AddComponent<MeshFilter>();
        rangeRend = rangeVisuals.AddComponent<MeshRenderer>();

        rangeRend.shadowCastingMode = ShadowCastingMode.Off;
        rangeRend.receiveShadows = false;

        if(rangeMat == null)
        {
            Debug.LogWarning("BuildableRangeDisplay: Range Material is not assigned");
            return;
        }

        rangeRend.material = new Material(rangeMat);

        ApplyRangeCircleColor();
    }

    void ApplyRangeCircleColor()
    {
        if (rangeRend == null || rangeRend.material == null)
        {
            return;
        }

        Material materialColor = rangeRend.material;

        if (materialColor.HasProperty("_BaseColor"))
        {
            materialColor.SetColor("_BaseColor", rangeColor);
        }
        else if (materialColor.HasProperty("_Color"))
        {
            materialColor.color = rangeColor;
        }

        materialColor.renderQueue = 3000;
    }

    float GetRangeRadius()
    {
        if(rangeSrc == BuildableRangeSrc.ManualRadius)
        {
            return Mathf.Max(0f, manualRadius);
        }

        if(rangeCollider == null)
        {
            return Mathf.Max(0f, manualRadius);
        }

        if(rangeSrc == BuildableRangeSrc.SphereCollider)
        {
            SphereCollider colliderSphere = rangeCollider as SphereCollider;

            if(colliderSphere == null)
            {
                return Mathf.Max(0f, manualRadius);
            }

            float xScale = Mathf.Abs(colliderSphere.transform.lossyScale.x);
            float yScale = Mathf.Abs(colliderSphere.transform.lossyScale.y);
            float zScale = Mathf.Abs(colliderSphere.transform.lossyScale.z);

            float maxScale = Mathf.Max(xScale, yScale, zScale);

            return colliderSphere.radius * maxScale;
        }

        if(rangeSrc == BuildableRangeSrc.BoxCollider)
        {
            BoxCollider colliderBox = rangeCollider as BoxCollider;

            if(colliderBox == null)
            {
                return Mathf.Max(0f, manualRadius);
            }

            Vector3 worldSize = Vector3.Scale(colliderBox.size, colliderBox.transform.lossyScale);

            float xRadius = Mathf.Abs(worldSize.x) * 0.5f;
            float zRadius = Mathf.Abs(worldSize.z) * 0.5f;

            return Mathf.Max(xRadius, zRadius);
        }

        return Mathf.Max(0f, manualRadius);
    }

    Vector3 GetRangeCenter()
    {
        if(rangeCollider == null)
        {
            return transform.position;
        }

        SphereCollider colliderSphere = rangeCollider as SphereCollider;

        if(colliderSphere != null)
        {
            return colliderSphere.transform.TransformPoint(colliderSphere.center);
        }

        BoxCollider colliderBox = rangeCollider as BoxCollider;

        if( colliderBox != null )
        {
            return colliderBox.transform.TransformPoint(colliderBox.center);
        }

        return rangeCollider.transform.position;
    }

    Vector3 GetRangeUpDir()
    {
        if(rangeCollider != null)
        {
            return rangeCollider.transform.up;
        }

        return transform.up;
    }

    Quaternion GetRangeRot()
    {
        if(rangeCollider != null)
        {
            return Quaternion.LookRotation(rangeCollider.transform.forward, rangeCollider.transform.up);
        }

        return Quaternion.LookRotation(transform.forward, transform.up);
    }

    void RebuildMeshIfNeed(float _Radius)
    {
        if(shownMesh != null)
        {
            bool radiusTheSame = Mathf.Approximately(_Radius, currMeshRadius);
            bool segmentCountTheSame = rangeCircles == currMeshSegment;

            if(radiusTheSame && segmentCountTheSame)
            {
                return;
            }
        }

        DestroyShownMesh();

        shownMesh = CreateCircleMesh(_Radius);
        rangeMeshFilter.sharedMesh = shownMesh;

        currMeshRadius = _Radius;
        currMeshSegment = rangeCircles;
    }

    void DestroyShownMesh()
    {
        if (shownMesh != null)
        {
            Destroy(shownMesh);
            shownMesh = null;
        }

        currMeshRadius = -1f;
        currMeshSegment = -1;
    }

    Mesh CreateCircleMesh(float _Radius)
    {
        Mesh circleMesh = new Mesh();
        circleMesh.name = "Range Circle Mesh";

        int safeSegmentCount = Mathf.Max(8, rangeCircles);

        Vector3[] vertices = new Vector3[safeSegmentCount + 1];
        int[] triangles = new int[safeSegmentCount * 3];

        vertices[0] = Vector3.zero;

        for(int i = 0; i < safeSegmentCount; i++)
        {
            float currAngle = ((float)i / safeSegmentCount) * Mathf.PI * 2f;

            float x = Mathf.Cos(currAngle) * _Radius;
            float z = Mathf.Sin(currAngle) * _Radius;

            vertices[i + 1] = new Vector3(x, 0f, z);
        }

        for(int i = 0; i < safeSegmentCount; i++)
        {
            int triangleIndex = i * 3;

            triangles[triangleIndex] = 0;

            int currOuterVertexIndex = i + 1;
            int nextOuterVertexIndex = i + 2;

            bool isLastTriangleSlice;

            if(i == safeSegmentCount - 1)
            {
                isLastTriangleSlice = true;
            }
            else
            {
                isLastTriangleSlice = false;
            }

            if (isLastTriangleSlice)
            {
                nextOuterVertexIndex = 1;
            }

            triangles[triangleIndex + 1] = nextOuterVertexIndex;
            triangles[triangleIndex + 2] = currOuterVertexIndex;

        }

        circleMesh.vertices = vertices;
        circleMesh.triangles = triangles;

        circleMesh.RecalculateNormals();
        circleMesh.RecalculateBounds();

        return circleMesh;
    }

    public bool WasSelectedColliderHit(Collider _Hit)
    {
        if(_Hit == null)
        {
            return false;
        }

        if(selectedColliders == null || selectedColliders.Length == 0)
        {
            return _Hit.transform.IsChildOf(transform);
        }

        for(int i = 0; i < selectedColliders.Length; i++)
        {
            if (selectedColliders[i] == null)
            {
                continue;
            }

            if(_Hit == selectedColliders[i])
            {
                return true;
            }
        }

        return false;
    }

    public bool IsRangeRend(Renderer _Renderer)
    {
        if(rangeRend == null)
        {
            return false;
        }

        return _Renderer == rangeRend;
    }

}
