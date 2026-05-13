using System;
using UnityEngine;

public class BuildableSelectionController : MonoBehaviour
{
    [Header("----- Ref -----")]
    [SerializeField] Camera selectCam;

    [Header("----- Select Settings -----")]
    [SerializeField] float selectDist = 10f;
    [SerializeField] KeyCode selectKey = KeyCode.Mouse1;
    [SerializeField] LayerMask selectMask = ~0;
    [SerializeField] QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

    [Header("----- Deselect Settings -----")]
    [SerializeField] bool deselectOnTurnAway = true;

    BuildableRangeDisplay selectedBuildable;

    private void Update()
    {
        if(selectCam == null)
        {
            return;
        }

        BuildableRangeDisplay viewedBuildable = GetViewedBuildable();

        if(selectedBuildable != null && deselectOnTurnAway && viewedBuildable != selectedBuildable)
        {
            ClearSelection();
        }

        if (Input.GetKeyDown(selectKey))
        {
            if (viewedBuildable != null)
            {
                SelectBuildable(viewedBuildable);
            }
            else
            {
                ClearSelection();
            }
        }
    }

    void SelectBuildable(BuildableRangeDisplay _Buildable)
    {
        if(_Buildable == null)
        {
            ClearSelection();
            return;
        }

        if(selectedBuildable == _Buildable)
        {
            selectedBuildable.ShowRange();
            return;
        }

        ClearSelection();

        selectedBuildable = _Buildable;
        selectedBuildable.ShowRange();
    }

    void ClearSelection()
    {
        if(selectedBuildable != null)
        {
            selectedBuildable.HideRange();
        }

        selectedBuildable = null;
    }

    BuildableRangeDisplay GetViewedBuildable()
    {
        Ray ray = selectCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        RaycastHit[] hits = Physics.RaycastAll(ray, selectDist, selectMask, triggerInteraction);

        if(hits == null || hits.Length == 0)
        {
            return null;
        }

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for(int i = 0; i < hits.Length; i++)
        {
            BuildableRangeDisplay currRangeDisplay = hits[i].collider.GetComponentInParent<BuildableRangeDisplay>();

            if(currRangeDisplay == null)
            {
                continue;
            }

            if (!currRangeDisplay.WasSelectedColliderHit(hits[i].collider))
            {
                continue;
            }

            return currRangeDisplay;
        }

        return null;
    }
}
