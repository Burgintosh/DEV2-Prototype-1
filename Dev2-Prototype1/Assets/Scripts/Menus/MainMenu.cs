using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] ButtonFunctions buttonFunctions;

    public GameObject mainMenu;
    public GameObject LevelSelectMenu;
    public GameObject EndlessMenu;
    public GameObject OptionsMenu;
    public GameObject ScoresMenu;
    public GameObject CreditsMenu;

    [SerializeField] GameObject menuParent;
    [SerializeField] string hoverColorHex = "#8C8C8C";

    private void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button btn in buttons)
        {
            ColorBlock colors = btn.colors;
            Color hoverColor;
            if (ColorUtility.TryParseHtmlString(hoverColorHex, out hoverColor))
            {
                colors.highlightedColor = hoverColor;
            }
            //colors.highlightedColor = 
        }
    }

    // Open Menus
    public void OpenLevelSelectMenu()
    {
        mainMenu.SetActive(false);
        LevelSelectMenu.SetActive(true);
    }
    public void OpenEndlessMenu()
    {
        mainMenu.SetActive(false);
        EndlessMenu.SetActive(true);
    }
    public void OpenOptionsMenu()
    {
        mainMenu.SetActive(false);
        OptionsMenu.SetActive(true);
    }
    public void OpenScoresMenu()
    {
        mainMenu.SetActive(false);
        ScoresMenu.SetActive(true);
    }
    public void OpenCreditsMenu()
    {
        mainMenu.SetActive(false);
        CreditsMenu.SetActive(true);
    }

    // Close Menus
    public void CloseLevelSelectMenu()
    {
        LevelSelectMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    public void CloseEndlessMenu()
    {
        EndlessMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    public void CloseOptionsMenu()
    {
        OptionsMenu.SetActive(false);
        mainMenu.SetActive(true);
        buttonFunctions.LoadSettings();
    }
    public void CloseScoresMenu()
    {
        ScoresMenu.SetActive(false);
        mainMenu.SetActive(true);
    }
    public void CloseCreditsMenu()
    {
        CreditsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }



    // Levels
    public void LoadLevelThree()
    {
        SceneManager.LoadScene("DiegoTestScene");
    }
    public void LoadLevelOne()
    {
        SceneManager.LoadScene("Level1");
    }
    public void LoadLevelTwo()
    {
        SceneManager.LoadScene("DiegoTestScene");
    }
    public void LoadLevelFour()
    {
        SceneManager.LoadScene("DiegoTestScene");
    }
    public void LoadShowcaseLevel()
    {
        SceneManager.LoadScene("ShowCaseLevelScene");
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
