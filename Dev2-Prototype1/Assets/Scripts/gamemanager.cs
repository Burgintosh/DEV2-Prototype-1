using UnityEngine;
using TMPro; // Access to text stuff
using UnityEngine.UI; // Access to UI stuff

public class gamemanager : MonoBehaviour
{
    // Basically core of game

    public static gamemanager instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuRespawn;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

    [SerializeField] private TextMeshProUGUI currencyText;

    public bool isPaused;
    public CurrencyManager currencyManager;

    public GameObject player;
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
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);

            }
            else if(menuActive == menuPause)
            {
                stateUnpause();
            }
        }
    }

    private void OnEnable()
    {
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged += UpdateUI;
        }
    }

    private void OnDisable()
    {
        if (currencyManager != null)
            currencyManager.OnCurrencyChanged -= UpdateUI;
    }

    public void statePause()
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
    
    public void youWin()
    {
        statePause();
        menuActive = menuPause;
        menuActive.SetActive(true);
    }

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive.SetActive(true);
    }

    //public void updateGameGoal(int amount)
    //{
    //    gameGoalCount += amount;

    //    if (gameGoalCount <= 0)
    //    {
    //        // winner winner chicken dinner
    //        statePause();
    //        menuActive = menuWin;
    //        menuActive.SetActive(true);
    //    }
    //}
    private void UpdateUI(int amount)
    {
        currencyText.text = $"${amount}";
    }

    public void Respawn()
    {

    }
}
