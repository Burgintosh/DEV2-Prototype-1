using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject LevelSelectMenu;
    public GameObject EndlessMenu;
    public GameObject OptionsMenu;
    public GameObject ScoresMenu;

    public void OpenLevelSelectMenu()
    {
        mainMenu.SetActive(false);
        LevelSelectMenu.SetActive(true);
    }
    public void CloseLevelSelectMenu()
    {
        LevelSelectMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    public void OpenEndlessMenu()
    {
        mainMenu.SetActive(false);
        EndlessMenu.SetActive(true);
    }
    public void CloseEndlessMenu()
    {
        EndlessMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    public void OpenOptionsMenu()
    {
        mainMenu.SetActive(false);
        OptionsMenu.SetActive(true);
    }
    public void CloseOptionsMenu()
    {
        OptionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    public void OpenScoresMenu()
    {
        mainMenu.SetActive(false);
        ScoresMenu.SetActive(true);
    }
    public void CloseScoresMenu()
    {
        ScoresMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    public void LoadLevelThree()
    {
        SceneManager.LoadScene("DiegoTestScene");
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
             Application.Quit();
#endif

    }
}
