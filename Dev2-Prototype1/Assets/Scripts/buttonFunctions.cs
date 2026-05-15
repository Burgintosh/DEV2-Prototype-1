using Newtonsoft.Json.Linq;
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

    private void Start()
    {
        LoadSettings();
    }
    private void LoadSettings()
    {
        float sens = PlayerPrefs.GetFloat(SENS_KEY, 500f);
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1.0f);
        float SFXVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1.0f);

        UpdateSensitivity(sens);
        UpdateMusicVolume(musicVol);
        UpdateSFXVolume(SFXVol);

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
        gamemanager.instance.CloseSettings();
    }


    public void UpdateSensitivity(float sens)
    {
        Debug.Log($"Slider moved! New Sensitivity is: {sens}");
        if (Camera.main != null)
        {
            cameraController camController = Camera.main.GetComponent<cameraController>();
            if (camController != null)
                camController.SetSensitivity(sens);
        }

        PlayerPrefs.SetFloat("MouseSensitivity", sens);
        PlayerPrefs.Save();
    }
    public void UpdateMasterVolume(float vol)
    {
        // For when we add Master Volume
    }

    public void UpdateMusicVolume(float vol)
    {
        Debug.Log($"Slider moved! New Music Volume is: {vol}");
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetMasterVolume(vol);

        PlayerPrefs.SetFloat("MusicVolume", vol);
        PlayerPrefs.Save();
    }

    public void UpdateSFXVolume(float vol)
    {
        Debug.Log($"Slider moved! New Music Volume is: {vol}");
        if (MusicManager.Instance != null)
            SoundManager.Instance.masterSFXVol = vol;

        PlayerPrefs.SetFloat("SFXVolume", vol);
        PlayerPrefs.Save();
    }
}
