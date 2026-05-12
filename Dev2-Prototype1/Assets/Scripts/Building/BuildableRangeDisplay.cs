using UnityEngine;

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

        if (materialColor.HasProperty("_Color"))
        {
            materialColor.color = rangeColor;
        }

        materialColor.renderQueue = 3000;
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
}
