using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonFunctions : MonoBehaviour
{
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


    public void UpdateSensitivity(float vol)
    {
        cameraController camController = Camera.main.GetComponent<cameraController>();
        if (camController != null)
            camController.SetSensitivity(vol);
    }
    public void UpdateMasterVolume(float vol)
    {
        // For when we add Master Volume
    }

    public void UpdateMusicVolume(float vol)
    {
        Debug.Log($"Slider moved! New Volume is: {vol}");
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetMasterVolume(vol);
    }

    public void UpdateSFXVolume(float vol)
    {
        if(MusicManager.Instance != null)
            SoundManager.Instance.masterSFXVol = vol;
    }
}
