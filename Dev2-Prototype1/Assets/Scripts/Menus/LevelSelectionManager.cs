using UnityEngine;
using UnityEngine.UI;

public class LevelSelectionManager : MonoBehaviour
{
    [SerializeField] private Button[] levelSelectButtons;
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
        if (levelSelectButtons.Length != endlessModeButtons.Length)
        {
            Debug.LogWarning("The number of buttons in the Level Select Array does not match the number of buttons in the" +
                " Endless Mode Array. Go to LevelSelectionManager.cs script on the MainMenuFunctions GameObject to resolve.");
        }

        int unlockedLevels = PlayerPrefs.GetInt(UNLOCKED_LEVELS_KEY, 1);
        for (int i = 0; i < levelSelectButtons.Length; i++)
        {
            if (i < unlockedLevels)
                levelSelectButtons[i].interactable = true;
            else
                levelSelectButtons[i].interactable = false;
            
            if (i + 1 < unlockedLevels) // Must clear the level first to unlock endless mode
                endlessModeButtons[i].interactable = true;
            else
                endlessModeButtons[i].interactable = false;
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
        int unlockedLevels = PlayerPrefs.GetInt(UNLOCKED_LEVELS_KEY);
        PlayerPrefs.SetInt(UNLOCKED_LEVELS_KEY, unlockedLevels + 1);
        PlayerPrefs.Save();
        UpdateLockedButtons();
    }
}
