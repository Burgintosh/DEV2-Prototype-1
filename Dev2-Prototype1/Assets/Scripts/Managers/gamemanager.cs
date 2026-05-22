using UnityEngine;
using TMPro; // Access to text stuff
using UnityEngine.UI;
using Unity.Properties; // Access to UI stuff
using UnityEngine.SceneManagement;

public class gamemanager : MonoBehaviour
{
    // Basically core of game
    private const string UNLOCKED_LEVELS_KEY = "UnlockedLevels";

    public static gamemanager instance;
    [SerializeField] ButtonFunctions buttonFunctions;

    [Header("Menus")]
    GameObject menuActive;
    [SerializeField] GameObject menuPause;
    // [SerializeField] GameObject menuRespawn;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuSetting;

    [Header("Currency")]
    public CurrencyManager currencyManager;
    [SerializeField] private TextMeshProUGUI currencyText;

    [Header("Gun UI")]
    [SerializeField] private TextMeshProUGUI AmmoCount;
    [SerializeField] private TextMeshProUGUI MagSize;
    
    [Header("Player HP UI")]
    public Image playerHPBar;
    public TextMeshProUGUI playerHPNum;
    public GameObject playerDamageFlashScreen;

    [Header("Nexus HP UI")]
    public Image NexusHPBar;

    [Header("Wave UI")]
    public WaveUIController waveUI;

    [Header("Build UI")]
    public GameObject sellPromptUI;

    [Header("Public Vars (Do Not Assign)")]
    public GameObject player;
    public playerController playerScript;
    public GameObject playerSpawnPos;
    public GameObject Nexus;
    public Nexus nexusScript;
    public bool isPaused;
    public Weapon activeWeapon;

    float timeScaleOrig; // So we can set pause game when pause menu is up. This lets us return to the time scale when unpausing

    int gameGoalCount;
    public int currScore = 0;
    [SerializeField] public uint TimeBonusMax;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() // Changing Start() to Awake() ensures this takes priority. Reserve Awake for manager types (Need this before other scripts run)
    {
        instance = this;
        Time.timeScale = 1f;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<playerController>();

        Nexus = GameObject.FindWithTag("Nexus");
        nexusScript = Nexus.GetComponent<Nexus>();

        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");
    }

    // Update is called once per frame
    void Update()
    {
        TimeBonusMax -= 1;
        if (Input.GetButtonDown("Cancel")) // defaulted to esc key in Unity
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);

            }
            else if (menuActive == menuSetting)
            {
                CloseSettings();
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
            currencyManager.OnCurrencyChanged += UpdateCurrencyUI;
        }
        //playerScript.GetCurrentWeapon().OnAmmoChange += UpdateAmmoUI;
        playerScript.OnWeaponChanged += UpdateGun;
        playerScript.OnHPChanged += UpdatePlayerHPBar;
        nexusScript.OnNexusHPChanged += UpdateNexusHPBar;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged -= UpdateCurrencyUI;
        }
        if (activeWeapon != null)
        {
            activeWeapon.OnAmmoChange -= UpdateAmmoUI;
        }

        if(playerScript != null)
        {
            if (playerScript.GetCurrentWeapon() != null)
                playerScript.GetCurrentWeapon().OnAmmoChange -= UpdateAmmoUI;
            playerScript.OnWeaponChanged -= UpdateGun;
            playerScript.OnHPChanged -= UpdatePlayerHPBar;
        }
        
        nexusScript.OnNexusHPChanged -= UpdateNexusHPBar;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void statePause(bool _LowerMusic = true)
    {
        isPaused = true;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if(_LowerMusic && MusicManager.Instance != null)
        {
            MusicManager.Instance.SetPausedMusicVol(true);
        }
    }

    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if(MusicManager.Instance != null)
        {
            MusicManager.Instance.SetPausedMusicVol(false);
        }

        menuActive.SetActive(false);
        menuActive = null;
    }
    public void OpenSettings()
    {
        if (menuActive != null)
        {
            menuActive.SetActive(false);
        }

        menuActive = menuSetting;
        menuActive.SetActive(true);
    }
    public void CloseSettings()
    {
        buttonFunctions.LoadSettings();
        if (menuSetting != null)
        {
            menuSetting.SetActive(false);
        }

        menuActive = menuPause;

        if (menuActive != null)
        {
            menuActive.SetActive(true);
        }
    }
    public void youWin()
    {
        int currentBuildIndex = SceneManager.GetActiveScene().buildIndex;
        int unlockedLevels = PlayerPrefs.GetInt(UNLOCKED_LEVELS_KEY, 1);
        if (currentBuildIndex >= unlockedLevels)
        {
            PlayerPrefs.SetInt(UNLOCKED_LEVELS_KEY, currentBuildIndex + 1);
            PlayerPrefs.Save();
            Debug.Log($"Level Completed! Unlocked Level {currentBuildIndex + 1}");
        }

        if(MusicManager.Instance != null)
        {
            MusicManager.Instance.SetPausedMusicVol(false);
            MusicManager.Instance.PlayMusic(MusicState.Victory, true);
        }

        statePause(false);
        menuActive = menuWin;
        menuActive.SetActive(true);
    }

    public void youLose()
    {
        if(MusicManager.Instance != null)
        {
            MusicManager.Instance.SetPausedMusicVol(false);
            MusicManager.Instance.PlayMusic(MusicState.Defeat, true);
        }

        statePause(false);
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
    private void UpdateCurrencyUI(int amount)
    {
        currencyText.text = $"${amount}";
    }

    private void UpdateAmmoUI(int amount)
    {
        AmmoCount.text = amount.ToString();
    }
    //private void UpdateMagSize(Weapon weapon)
    //{
    //    MagSize.text = weapon.magazineSize.ToString();
    //    UpdateAmmoUI(weapon.bulletsLeft);
    //}
    private void UpdateGun(Weapon weapon)
    {

        if (activeWeapon != null)
            activeWeapon.OnAmmoChange -= UpdateAmmoUI;

        activeWeapon = weapon;

        //playerScript.GetLastWeapon().gameObject.SetActive(false);
        //playerScript.GetCurrentWeapon().gameObject.SetActive(true);
        if (activeWeapon != null)
        {
            activeWeapon.OnAmmoChange += UpdateAmmoUI;
            MagSize.text = activeWeapon.data.magazineSize.ToString();
            UpdateAmmoUI(activeWeapon.data.bulletsLeft);
        }
    }

    private void UpdatePlayerHPBar(int HP)
    {
        playerHPBar.fillAmount = (float)HP / playerScript.GetMaxHP();
        playerHPNum.text = HP.ToString();
    }

    private void UpdateNexusHPBar(int HP)
    {
        NexusHPBar.fillAmount = (float)HP / NexusManager.nexusManagerInstance.totalNexusHealth;
    }

    public void UpdateNexusHPBar2()
    {
        if (NexusManager.nexusManagerInstance.totalNexusHealth <= 0)
        {
            NexusManager.nexusManagerInstance.countTotalHealth();
        }
        NexusManager.nexusManagerInstance.checkCurrHealth();
        if (NexusManager.nexusManagerInstance.nexusCount == 0)
        {
            NexusHPBar.fillAmount = 0;
        }
        else
        {
            NexusHPBar.fillAmount = (float)NexusManager.nexusManagerInstance.currNexusHealth / NexusManager.nexusManagerInstance.totalNexusHealth;
        }
            
    }
    public void Respawn()
    {

    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateGun(playerScript.GetCurrentWeapon());
        UpdatePlayerHPBar(playerScript.GetCurrentHP());


    }
}
