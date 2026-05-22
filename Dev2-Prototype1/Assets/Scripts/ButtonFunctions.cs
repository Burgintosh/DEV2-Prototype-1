using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ButtonFunctions : MonoBehaviour
{
    private const string SENS_KEY = "MouseSensitivity";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string CAMERA_SHAKE_KEY = "CameraShakeEnabled";
    private const string SCREEN_RESOLUTION_KEY = "ScreenResolution";
    private const string FULLSCREEN_KEY = "FullScreen";


    //Input Settings
    [SerializeField] private Slider sensitivitySlider;

    // Sound Settings
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    //Accessibility Settings
    [SerializeField] private Toggle cameraShakeToggle;
    
    // Display Settings
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    // For disabling these on WebGL
    [SerializeField] private GameObject FullScreenParent;
    [SerializeField] private GameObject ScreenResolutionParent;

    private float pendingSens;
    private float pendingMusicVol;
    private float pendingSFXVol;
    private bool pendingCameraShake;

    private Resolution[] availableResolutions;
    private int pendingResolutionIndex;
    private bool pendingFullscreen;

    private void Start()
    {
        PopulateResolutionDropdown();
        AdjustUIForPlatform();
        LoadSettings();
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        gamemanager.instance.stateUnpause();
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
        // Ensure resolutions are populated
        if (availableResolutions == null || availableResolutions.Length == 0)
            PopulateResolutionDropdown();

        float savedSens = PlayerPrefs.GetFloat(SENS_KEY, 10f);
        float savedMusicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
        float savedSFXVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1.0f);

        int savedCamShakeInt = PlayerPrefs.GetInt(CAMERA_SHAKE_KEY, 1);
        bool savedCamShakeBool = savedCamShakeInt > 0; // converting cameraShake's saved int back to bool

        int savedResolutionIndex = PlayerPrefs.GetInt(SCREEN_RESOLUTION_KEY, -1);
        bool savedFullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, Screen.fullScreen ? 1 : 0) > 0;

        // If there's not a saved screen res index yet, have to find it/use last available
        if(savedResolutionIndex < 0 || savedResolutionIndex > availableResolutions.Length)
        {
            int bestMatchIndex = 0;
            for (int i = 0; i < availableResolutions.Length; ++i)
            {
                Resolution resolutionToCheck = availableResolutions[i];
                if(resolutionToCheck.width == Screen.currentResolution.width && resolutionToCheck.height == Screen.currentResolution.height)
                {
                    bestMatchIndex = i;
                    //break;
                }
            }
            savedResolutionIndex = bestMatchIndex;
        }

        pendingSens = savedSens;
        pendingMusicVol = savedMusicVol;
        pendingSFXVol = savedSFXVol;
        pendingCameraShake = savedCamShakeBool;
        pendingResolutionIndex = savedResolutionIndex;
        pendingFullscreen = savedFullscreen;


        ApplySettings();

        if (sensitivitySlider != null)
            sensitivitySlider.value = pendingSens;

        if (musicSlider != null)
            musicSlider.value = pendingMusicVol;

        if (sfxSlider != null)
            sfxSlider.value = pendingSFXVol;

        if (cameraShakeToggle != null)
            cameraShakeToggle.isOn = pendingCameraShake;

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> ResolutionOptions = new List<string>();
            for (int i = 0; i < availableResolutions.Length; i++)
            {
                Resolution resolution = availableResolutions[i];
                ResolutionOptions.Add(string.Format("{0} x {1}", resolution.width, resolution.height));
            }
            resolutionDropdown.AddOptions(ResolutionOptions);
            resolutionDropdown.value = Mathf.Clamp(pendingResolutionIndex, 0, availableResolutions.Length - 1);
            resolutionDropdown.RefreshShownValue();
        }

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = pendingFullscreen;
    }
    public void ApplySettings()
    {
        if(!IsRunningOnWebGL())
        {
            if (availableResolutions != null && availableResolutions.Length > 0)
            {
                int resolutionIndex = Mathf.Clamp(pendingResolutionIndex, 0, availableResolutions.Length - 1);
                Resolution resolution = availableResolutions[resolutionIndex];
                Screen.SetResolution(resolution.width, resolution.height, pendingFullscreen);
            }
            else
                Debug.Log("No available resolutions found - (ButtonFunctions.ApplySettings())");
        }
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
        int pendingFullscreenInt = pendingFullscreen ? 1 : 0;

        PlayerPrefs.SetFloat(SENS_KEY, pendingSens);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, pendingMusicVol);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, pendingSFXVol);
        PlayerPrefs.SetInt(CAMERA_SHAKE_KEY, pendingCameraShakeInt);
        PlayerPrefs.SetInt(SCREEN_RESOLUTION_KEY, pendingResolutionIndex);
        PlayerPrefs.SetInt(FULLSCREEN_KEY, pendingFullscreenInt);
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
    public void UpdateResolution(int resolutionIndex)
    {
        Debug.Log($"Resolution changed! New index is: {resolutionIndex} - UpdateResolution(int resolutionIndex)");
        
        pendingResolutionIndex = resolutionIndex;
    }
    public void UpdateFullscreenToggle(bool isEnabled)
    {
        Debug.Log($"Fullscreen Toggle! New state: {isEnabled}");
        
        pendingFullscreen = isEnabled;
    }

    private void PopulateResolutionDropdown()
    {
        if (IsRunningOnWebGL()) return;

        availableResolutions = Screen.resolutions;
        if (availableResolutions == null || availableResolutions.Length == 0)
        {
            // Fallback: create at least current resolution
            Resolution defaultResolution = Screen.currentResolution;
            availableResolutions = new Resolution[] { defaultResolution };
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            for (int i = 0; i < availableResolutions.Length; ++i)
            {
                Resolution resolution = availableResolutions[i];
                options.Add(string.Format("{0} x {1}", resolution.width, resolution.height));
            }
            resolutionDropdown.AddOptions(options);
        }
    }


    public void MainMenu()
    {
        LoadingManager.Instance.StartLoading("MainMenu");
        gamemanager.instance.stateUnpause();
        //SceneManager.LoadScene("MainMenu");
    }

    private void AdjustUIForPlatform()
    {
        bool isWeb = IsRunningOnWebGL();

        if (fullscreenToggle != null)
            FullScreenParent.gameObject.SetActive(!isWeb);

        if (resolutionDropdown != null)
            ScreenResolutionParent.gameObject.SetActive(!isWeb);
    }
    private bool IsRunningOnWebGL()
    {
        return Application.platform == RuntimePlatform.WebGLPlayer;
    }

}