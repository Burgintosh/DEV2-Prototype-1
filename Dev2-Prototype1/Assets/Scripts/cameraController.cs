using UnityEngine;

public class cameraController : MonoBehaviour
{
    [SerializeField] int sens;
    [SerializeField] int lockVertMin, lockVertMax; // Stops from looking too far up/down and inverting the camera
    [SerializeField] bool invertY;
    [SerializeField] Transform player;

    float camRotX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; // Cursor won't move. Won't click out of window if have multiple monitors
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sens * Time.deltaTime; // mousey = camerax

        if(invertY)
            camRotX += mouseY;
        else
            camRotX -= mouseY;



            camRotX = Mathf.Clamp(camRotX, lockVertMin, lockVertMax);
        transform.localRotation = Quaternion.Euler(camRotX, 0, 0); // Use Quaternion library when rotating ANYTHING

        player.transform.Rotate(Vector3.up * mouseX); // Vector3.up = y axis
    }
}
