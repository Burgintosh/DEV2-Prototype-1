using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelSelectionManager : MonoBehaviour
{
    [SerializeField] private Button[] levelSelectButtons;
    [SerializeField] private Button[] endlessModeButtons;

    private const string LOCK_ICON = "LockIcon";
    private const string UNLOCKED_LEVELS_KEY = "UnlockedLevels";

    private const string LAST_VIEWED_LEVELSELECT_KEY = "LastViewed_LevelSelect";
    private const string LAST_VIEWED_ENDLESS_KEY = "LastViewed_Endless";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //void Start()
    //{
    //    UpdateLockedButtons();
    //}
    public void OnLevelSelectOpened()
    {
        UpdateLockedButtons();
        AnimateNewUnlocksLevelSelect();
    }

    public void OnEndlessOpened()
    {
        UpdateLockedButtons();
        AnimateNewUnlocksEndless();
    }

    // Level progress is saved as an int. This grabs that int,
    // then for each button that leads to a level,
    // checks it's position in the button array vs the number of unlocked levels.
    private void UpdateLockedButtons()
    {
        //if (levelSelectButtons.Length != endlessModeButtons.Length)
        //{
        //    Debug.LogWarning("The number of buttons in the Level Select Array does not match the number of buttons in the" +
        //        " Endless Mode Array. Go to LevelSelectionManager.cs script on the MainMenuFunctions GameObject to resolve.");
        //}

        int unlockedLevels = PlayerPrefs.GetInt(UNLOCKED_LEVELS_KEY, 1);

        for (int i = 0; i < levelSelectButtons.Length; ++i)
        {
            bool isLevelUnlocked = i < unlockedLevels;
            SetButtonState(levelSelectButtons[i], isLevelUnlocked);

            bool isEndlessUnlocked = i + 1 < unlockedLevels;
            SetButtonState(endlessModeButtons[i], isEndlessUnlocked);
        }
    }

    private void SetButtonState(Button button, bool isUnlocked) 
    {
        if (button == null) return;
        
        button.interactable = isUnlocked;

        Transform lockIcon = button.transform.Find(LOCK_ICON);
        if (lockIcon != null)
            lockIcon.gameObject.SetActive(!isUnlocked);
    }

    private void AnimateNewUnlocksLevelSelect()
    {
        int unlockedLevels = PlayerPrefs.GetInt(UNLOCKED_LEVELS_KEY, 1);
        int lastViewed = PlayerPrefs.GetInt(LAST_VIEWED_LEVELSELECT_KEY, 0);
        if (lastViewed < unlockedLevels)
        {
            int start = Mathf.Clamp(lastViewed, 0, levelSelectButtons.Length);
            int end = Mathf.Min(unlockedLevels, levelSelectButtons.Length);
            for (int i = start; i < end; i++)
            {
                if (levelSelectButtons[i] != null)
                {
                    Transform lockIcon = levelSelectButtons[i].transform.Find(LOCK_ICON);
                    if (lockIcon != null)
                    {
                        lockIcon.gameObject.SetActive(true);
                        StartCoroutine(ShakeAndUnlock(lockIcon));
                    }
                }
            }
            PlayerPrefs.SetInt(LAST_VIEWED_LEVELSELECT_KEY, unlockedLevels);
            PlayerPrefs.Save();
        }
    }

    private void AnimateNewUnlocksEndless()
    {
        int unlockedLevels = PlayerPrefs.GetInt(UNLOCKED_LEVELS_KEY, 1);
        int unlockedEndless = 0;
        if (unlockedLevels - 1 >= 0)
            unlockedEndless = unlockedLevels - 1;

        int lastViewed = PlayerPrefs.GetInt(LAST_VIEWED_ENDLESS_KEY, 0);
        if (lastViewed < unlockedEndless)
        {
            int start = Mathf.Clamp(lastViewed, 0, endlessModeButtons.Length);
            int end = Mathf.Min(unlockedEndless, endlessModeButtons.Length);
            for (int i = start; i < end; i++)
            {
                if (endlessModeButtons[i] != null)
                {
                    Transform lockIcon = endlessModeButtons[i].transform.Find(LOCK_ICON);
                    if (lockIcon != null)
                    {
                        lockIcon.gameObject.SetActive(true);
                        StartCoroutine(ShakeAndUnlock(lockIcon));
                    }
                }
            }
            PlayerPrefs.SetInt(LAST_VIEWED_ENDLESS_KEY, unlockedEndless);
            PlayerPrefs.Save();
        }
    }
    private IEnumerator ShakeAndUnlock(Transform lockIcon)
    {
        if (lockIcon == null) yield break;
        RectTransform lockTransform = lockIcon as RectTransform;
        Vector3 posOrig = lockTransform != null ? lockTransform.anchoredPosition3D : lockIcon.localPosition;

        float duration = 0.6f;
        float elapsed = 0f;
        float magnitude = 8f;

        lockIcon.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            float dam = Mathf.Lerp(magnitude, 0f, elapsed / duration);
            Vector2 offset = Random.insideUnitCircle * dam;
            if (lockTransform != null)
                lockTransform.anchoredPosition3D = posOrig + new Vector3(offset.x, offset.y, 0f);
            else
                lockIcon.localPosition = posOrig + new Vector3(offset.x, offset.y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (lockTransform != null) lockTransform.anchoredPosition3D = posOrig;
        else lockIcon.localPosition = posOrig;

        lockIcon.gameObject.SetActive(false);
    }

    // TESTING

    public void ResetProgress()
    {
        PlayerPrefs.SetInt(UNLOCKED_LEVELS_KEY, 1);
        PlayerPrefs.SetInt(LAST_VIEWED_LEVELSELECT_KEY, 1);
        PlayerPrefs.SetInt(LAST_VIEWED_ENDLESS_KEY, 0);
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
