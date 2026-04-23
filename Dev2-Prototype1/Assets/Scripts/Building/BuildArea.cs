using UnityEngine;

public class BuildArea : MonoBehaviour
{
    [SerializeField] BoxCollider areaCollider;
    [SerializeField] BuildableType[] allowedBuildTypes;
    [SerializeField] bool showDebugGizmos = true;

    private void Reset()
    {
        areaCollider = GetComponent<BoxCollider>();
    }

    private void Awake()
    {
        if(areaCollider == null)
        {
            areaCollider = GetComponent<BoxCollider>();
        }
    }

    public bool AllowsBuildType(BuildableType _BuildType)
    {
        for(int i = 0; i < allowedBuildTypes.Length; i++)
        {
            if(allowedBuildTypes[i] == _BuildType)
            {
                return true;
            }
        }

        return false;
    }

    public BoxCollider GetAreaCollider()
    {
        return areaCollider;
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos)
        {
            return;
        }

        BoxCollider box;

        if(areaCollider != null)
        {
            box = areaCollider;
        }
        else
        {
            box = GetComponent<BoxCollider>();
        }

        if(box == null)
        {
            return;
        }

        Gizmos.color = new Color(0f, 0.5f, 1f, 0.2f);
        Matrix4x4 oldMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);
        Gizmos.color = new Color(0f, 0.5f, 1f, 1f);
        Gizmos.DrawWireCube(box.center, box.size);
        Gizmos.matrix = oldMatrix;
    }

}
