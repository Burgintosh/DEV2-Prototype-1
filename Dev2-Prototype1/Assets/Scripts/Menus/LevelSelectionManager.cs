using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionManager : MonoBehaviour
{
    [Header("Level Buttons")]
    [SerializeField] private Button[] levelSelectButtonsButtons;
    [SerializeField] private Button[] endlessModeButtons;
    private const string UNLOCKED_LEVELS_KEY = "UnlockedLevels";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateLockedButtons();
    }

    // Level progress is saved as an int. This grabs that int,
    // then for each button that leads to a level,
    // checks it's position in the button array vs the number of unlocked levels.
    private void UpdateLockedButtons()
    {
        if (levelSelectButtonsButtons.Length != endlessModeButtons.Length)
        {
            Debug.LogWarning("The number of buttons in the Level Select Array does not match the number of buttons in the" +
                " Endless Mode Array. Go to LevelSelectionManager.cs script on the MainMenuFunctions GameObject to resolve.");
        }

        int unlockedLevels = PlayerPrefs.GetInt(UNLOCKED_LEVELS_KEY);
        for (int i = 0; i < levelSelectButtonsButtons.Length; i++)
        {
            if (i < unlockedLevels)
            {
                levelSelectButtonsButtons[i].interactable = true;
                endlessModeButtons[i].interactable = true;
            }
            else
            {
                levelSelectButtonsButtons[i].interactable = false;
                endlessModeButtons[i].interactable = false;
            }
        }

        //for (int i = 0; i < endlessModeButtons.Length; i++)
        //{
        //    if (i < unlockedLevels)
        //    {
        //        endlessModeButtons[i].interactable = true;
        //    }
        //    else
        //    {
        //        endlessModeButtons[i].interactable = false;
        //    }
        //}
    }



    // TESTING

    public void ResetProgress()
    {
        PlayerPrefs.SetInt(UNLOCKED_LEVELS_KEY, 1);
        PlayerPrefs.Save();
        UpdateLockedButtons();
    }

    public void IncrementProgress()
    {
        PlayerPrefs.SetInt(UNLOCKED_LEVELS_KEY + 1, 1);
        PlayerPrefs.Save();
        UpdateLockedButtons();
    }
}
