using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonFunctions : MonoBehaviour
{
    private const string SENS_KEY = "MouseSensitivity";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string CAMERA_SHAKE_KEY = "CameraShakeEnabled";

    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle cameraShakeToggle;

    private float pendingSens;
    private float pendingMusicVol;
    private float pendingSFXVol;
    private bool pendingCameraShake;

    private void Start()
    {
        LoadSettings();
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void resume()
    {
        gamemanager.instance.stateUnpause();
    }
    public void restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        gamemanager.instance.stateUnpause();
    }
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif

    }
    public void Settings()
    {
        gamemanager.instance.OpenSettings();
    }
    public void CloseSettings()
    {
        LoadSettings();
        gamemanager.instance.CloseSettings();
    }



    // SETTINGS FUNCTIONS
    public void LoadSettings()
    {
        float sens = PlayerPrefs.GetFloat(SENS_KEY, 10f);
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
        float SFXVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1.0f);
        int camShakeInt = PlayerPrefs.GetInt(CAMERA_SHAKE_KEY, 1);
        bool camShakeBool = camShakeInt > 0; // converting cameraShake's saved int back to bool

        pendingSens = sens;
        pendingMusicVol = musicVol;
        pendingSFXVol = SFXVol;
        pendingCameraShake = camShakeBool;

        ApplySettings();

        if (sensitivitySlider != null)
            sensitivitySlider.value = sens;

        if (musicSlider != null)
            musicSlider.value = musicVol;

        if (sfxSlider != null)
            sfxSlider.value = SFXVol;

        if (cameraShakeToggle != null)
            cameraShakeToggle.isOn = camShakeBool;
    }
    public void ApplySettings()
    {
        if (Camera.main != null)
        {
            cameraController camController = Camera.main.GetComponent<cameraController>();
            if (camController != null)
            {
                camController.SetSensitivity(pendingSens);
                camController.SetCameraShake(pendingCameraShake);
            }
        }
        else
            Debug.Log("Camera doesn't exist??? (ButtonFunctions.ApplySettings())");

        if (MusicManager.Instance != null)
            MusicManager.Instance.SetMasterVolume(pendingMusicVol);
        else
            Debug.Log("Music Manager doesn't exist yet (ButtonFunctions.ApplySettings())");

        if (SoundManager.Instance != null)
            SoundManager.Instance.masterSFXVol = pendingSFXVol;
        else
            Debug.Log("Sound Manager doesn't exist yet (ButtonFunctions.ApplySettings())");

    }
    public void ApplyAndSaveSettings()
    {
        ApplySettings();

        int pendingCameraShakeInt = pendingCameraShake ? 1 : 0;

        PlayerPrefs.SetFloat(SENS_KEY, pendingSens);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, pendingMusicVol);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, pendingSFXVol);
        PlayerPrefs.SetInt(CAMERA_SHAKE_KEY, pendingCameraShakeInt);
        PlayerPrefs.Save();

        Debug.Log("Settings Applied and Saved! (ButtonFunctions.ApplyAndSaveSettings())");
    }

    public void UpdateSensitivity(float sens)
    {
        Debug.Log($"Slider moved! New Sensitivity is: {sens} - ButtonFunctions.UpdateSensitivity(float sens))");

        pendingSens = sens;
    }
    public void UpdateMasterVolume(float vol)
    {
        // For when we add Master Volume
    }

    public void UpdateMusicVolume(float vol)
    {
        Debug.Log($"Slider moved! New Music Volume is: {vol} - ButtonFunctions.UpdateMusicVolume(float vol)");

        pendingMusicVol = vol;
    }

    public void UpdateSFXVolume(float vol)
    {
        Debug.Log($"Slider moved! New Music Volume is: {vol} - UpdateSFXVolume(float vol)");

        pendingSFXVol = vol;
    }

    public void UpdateCameraShakeToggle(bool isEnabled)
    {
        Debug.Log($"Camera Shake Toggle! New toggle state is: {isEnabled} - UpdateCameraShakeToggle(bool isEnabled)");
        
        pendingCameraShake = isEnabled;
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}