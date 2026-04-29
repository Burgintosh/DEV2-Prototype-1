using UnityEngine;
using TMPro; // Access to text stuff
using UnityEngine.UI;
using Unity.Properties; // Access to UI stuff

public class gamemanager : MonoBehaviour
{
    // Basically core of game

    public static gamemanager instance;

    [Header("Menus")]
    GameObject menuActive;
    [SerializeField] GameObject menuPause;
    // [SerializeField] GameObject menuRespawn;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake() // Changing Start() to Awake() ensures this takes priority. Reserve Awake for manager types (Need this before other scripts run)
    {
        instance = this;
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
            currencyManager.OnCurrencyChanged += UpdateCurrencyUI;
        }
        //playerScript.GetCurrentWeapon().OnAmmoChange += UpdateAmmoUI;
        playerScript.OnWeaponChanged += UpdateGun;
        playerScript.OnHPChanged += UpdatePlayerHPBar;
        nexusScript.OnNexusHPChanged += UpdateNexusHPBar;
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

        playerScript.GetCurrentWeapon().OnAmmoChange -= UpdateAmmoUI;
        playerScript.OnWeaponChanged -= UpdateGun;
        playerScript.OnHPChanged -= UpdatePlayerHPBar;
        nexusScript.OnNexusHPChanged -= UpdateNexusHPBar;
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
        menuActive = menuWin;
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
        NexusHPBar.fillAmount = (float)NexusManager.nexusManagerInstance.nexusCount / NexusManager.nexusManagerInstance.nexusList.Count;
    }


    public void Respawn()
    {

    }
}
