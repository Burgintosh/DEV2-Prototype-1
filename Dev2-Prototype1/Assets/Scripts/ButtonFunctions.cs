using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonFunctions : MonoBehaviour
{
    private const string SENS_KEY = "MouseSensitivity";
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private float pendingSens;
    private float pendingMusicVol;
    private float pendingSFXVol;

    private void Start()
    {
        LoadSettings();
    }
    public void LoadSettings()
    {
        float sens = PlayerPrefs.GetFloat(SENS_KEY, 500f);
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
        float SFXVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1.0f);

        pendingSens = sens;
        pendingMusicVol = musicVol;
        pendingSFXVol = SFXVol;

        ApplySettings();

        if (sensitivitySlider != null)
            sensitivitySlider.value = sens;

        if (musicSlider != null)
            musicSlider.value = musicVol;

        if (sfxSlider != null)
            sfxSlider.value = SFXVol;
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

    public void ApplySettings()
    {
        if (Camera.main != null)
        {
            cameraController camController = Camera.main.GetComponent<cameraController>();
            if (camController != null)
                camController.SetSensitivity(pendingSens);
        }

        if (MusicManager.Instance != null)
            MusicManager.Instance.SetMasterVolume(pendingMusicVol);
        else
            Debug.Log("Music Manager doesn't exist yet");

        if (SoundManager.Instance != null)
            SoundManager.Instance.masterSFXVol = pendingSFXVol;
    }
    public void ApplyAndSaveSettings()
    {
        ApplySettings();

        PlayerPrefs.SetFloat(SENS_KEY, pendingSens);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, pendingMusicVol);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, pendingSFXVol);
        PlayerPrefs.Save();

        Debug.Log("Settings Applied and Saved!");
    }

    public void UpdateSensitivity(float sens)
    {
        Debug.Log($"Slider moved! New Sensitivity is: {sens}");

        pendingSens = sens;
    }
    public void UpdateMasterVolume(float vol)
    {
        // For when we add Master Volume
    }

    public void UpdateMusicVolume(float vol)
    {
        Debug.Log($"Slider moved! New Music Volume is: {vol}");

        pendingMusicVol = vol;
    }

    public void UpdateSFXVolume(float vol)
    {
        Debug.Log($"Slider moved! New Music Volume is: {vol}");

        pendingSFXVol = vol;
    }
}