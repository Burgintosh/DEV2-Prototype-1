using UnityEngine;
using TMPro; // Access to text stuff
using UnityEngine.UI;

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
    [SerializeField] private TextMeshProUGUI AmmoCount;
    [SerializeField] private TextMeshProUGUI MagSize;

    public GameObject playerDamageFlashScreen;

    public bool isPaused;
    public Image playerHealthBar;
    public GameObject playerDamageFlash;
    public CurrencyManager currencyManager;
    public Weapon activeWeapon;

    public GameObject player;
    public playerController playerScript;
    public GameObject Nexus;
   
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
        playerScript.GetCurrentWeapon().OnAmmoChange += UpdateAmmoUI;
        playerScript.OnWeaponChanged += UpdateGun;
    }

    private void OnDisable()
    {
        if (currencyManager != null)
        {
            currencyManager.OnCurrencyChanged -= UpdateCurrencyUI;
        }
        playerScript.GetCurrentWeapon().OnAmmoChange -= UpdateAmmoUI;
        playerScript.OnWeaponChanged -= UpdateGun;
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
        playerScript.GetLastWeapon().gameObject.SetActive(false);
        playerScript.GetCurrentWeapon().gameObject.SetActive(true);
        MagSize.text = weapon.magazineSize.ToString();
        UpdateAmmoUI(weapon.bulletsLeft);
    }
    

    public void Respawn()
    {

    }
}
