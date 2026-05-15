using System.Collections;
using UnityEngine;

public class cameraController : MonoBehaviour
{
    [SerializeField] float sens;
    [SerializeField] int lockVertMin, lockVertMax; // Stops from looking too far up/down and inverting the camera
    [SerializeField] bool invertY;
    [SerializeField] Transform player;

    float camRotX;

    private Coroutine shakeCoroutine;
    private Quaternion shakeRotationOffset = Quaternion.identity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked; // Cursor won't move. Won't click out of window if have multiple monitors
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * sens * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sens * Time.deltaTime; // mousey = camerax
        //float mouseX = Input.GetAxisRaw("Mouse X") * sens * .3f;
        //float mouseY = Input.GetAxisRaw("Mouse Y") * sens * .3f; // mousey = camerax

        if(invertY)
            camRotX += mouseY;
        else
            camRotX -= mouseY;



        camRotX = Mathf.Clamp(camRotX, lockVertMin, lockVertMax);
        transform.localRotation = Quaternion.Euler(camRotX, 0, 0) * shakeRotationOffset; // Use Quaternion library when rotating ANYTHING

        if(player != null)
            player.transform.Rotate(Vector3.up * mouseX); // Vector3.up = y axis
    }

    public void SetSensitivity(float _sens)
    {
        sens = _sens;
    }

    public void Shake(float duration, float magnitude, float frequency = 20f)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude, frequency));


    }
    private IEnumerator ShakeRoutine(float duration, float magnitude, float frequency)
    {
        float shakeTimer = 0.0f;

        float seedX = Random.value * 100f;
        float seedY = Random.value * 100f;
        float seedZ = Random.value * 100f;

        while (shakeTimer < duration)
        {
            float timeStep = shakeTimer * frequency;

            float xOffset = (Mathf.PerlinNoise(seedX + timeStep, 0f) * 2f - 1f) * magnitude;
            float yOffset = (Mathf.PerlinNoise(0f, seedY + timeStep) * 2f - 1f) * magnitude;
            float zOffset = (Mathf.PerlinNoise(seedZ + timeStep, seedZ + timeStep) * 2f - 1f) * magnitude;

            float damping = 1 - (shakeTimer / duration);

            shakeRotationOffset = Quaternion.Euler(xOffset * damping, yOffset * damping, zOffset * damping);

            shakeTimer += Time.unscaledDeltaTime;

            yield return null;
        }

        shakeRotationOffset = Quaternion.identity;
        shakeCoroutine = null;
    }

}
