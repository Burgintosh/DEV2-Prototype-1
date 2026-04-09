using UnityEngine;
using TMPro; // Access to text stuff
using UnityEngine.UI; // Access to UI stuff

public class gamemanager : MonoBehaviour
{
    // Basically core of game

    public static gamemanager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuWin;

    public GameObject player;
    public bool isPaused;
    // [SerializeField] TMP_Text text; // Not sure if I hallucinated this
    public playerController playerScript;

    float timeScaleOrig; // So we can set pause game when pause menu is up. This lets us return to the time scale when unpausing

    int gameGoalCount;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() // Changing Start() to Awake() ensures this takes priority. Reserve Awake for manager types (Need this before other scripts run)
    {
        instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel")) // defaulted to esc key in Unity
        {
            if (menuActive == null)
            {
                statepause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else
            {
                if (menuActive == menuPause)
                {
                    stateUnpause();
                }
            }
        }
    }

    public void statepause()
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void youLose()
    {
        statepause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    public void updateGameGoal(int amount)
    {
        gameGoalCount += amount;

        if (gameGoalCount <= 0)
        {
            // winner winner chicken dinner
            statepause();
            menuActive = menuWin;
            menuActive.SetActive(true);
        }
    }
}
