using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AutoStackOverlayCamera : MonoBehaviour
{
    // This is needed since the main camera is in a separate prefab from the ui camera.
    // Because of that we can't just drag the ui cam onto the main cam's additional camera list like we did with the gun cam
    // Went ahead and made it modular so if we add more cameras we can throw this on them
    void Start()
    {
        Camera cam = GetComponent<Camera>();

        var camData = cam.GetUniversalAdditionalCameraData();
        camData.renderType = CameraRenderType.Overlay; // Ensures the overlay camera is overlay so it doesn't interfere with the main camera

        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            var mainCamData = mainCam.GetUniversalAdditionalCameraData();

            if (!mainCamData.cameraStack.Contains(cam))
                mainCamData.cameraStack.Add(cam);
        }
        //else
        //{
        //    Debug.LogWarning("No Main Camera found! Overlay Camera couldn't be stacked.");
        //}
    }
}
