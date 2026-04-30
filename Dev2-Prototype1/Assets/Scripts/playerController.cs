using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerController : MonoBehaviour, IDamage, IPickup
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("General Stats")]
    [Range(1, 200)] [SerializeField] int HP;
    int HPOrig;


    [Header("Gun")]
    [SerializeField] List<Weapon> weapons = new List<Weapon>();
    private List<Weapon> weaponModels = new List<Weapon>();
    [SerializeField] private GameObject weaponHolder;
    private Weapon lastWeapon;
    private Weapon currentWeapon;
    int currentWeaponIndex = 0;
    float shootTimer;
   

    [Header("Dash")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashTime;
    [SerializeField] float dashCooldown;
    float dashTimer;
    float dashCooldownTimer;
    bool isDashing;
    Vector3 dashDir;


    [Header("General Movement")]
    [Range(3, 7)][SerializeField] float speed;
    [Range(2, 5)][SerializeField] float sprintMod;
    [Range(5, 25)][SerializeField] float jumpSpeed;
    [Range(1, 3)][SerializeField] int jumpMax;
    [Range(15, 50)][SerializeField] float gravity;
    int jumpCount;

    [Header("New Movement Stats")]
    [Range(50, 200)] [SerializeField] float acceleration = 75f; // Default 75
    [Range(100, 300)] [SerializeField] float airAcceleration = 150f; // Default 150
    [Range(1, 30)] [SerializeField] float groundFriction = 25f; // Default 25
    [Range(0.1f, 0.5f)] [SerializeField] float jumpBuffer = 0.4f; // Default 0.4
    [Range(0.1f, 0.3f)][SerializeField] float coyoteTime = 0.15f; // 0.15
    [Range(0f, 10f)] [SerializeField] float airSpeedCap = 1f; // Default 1 (0 for no cap)
    float jumpBufferTimer;
    float coyoteTimer;


    [Header("Input Setup (For Input System Package")]
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference jumpAction;
    [SerializeField] InputActionReference sprintAction;
    [SerializeField] InputActionReference dashAction;
    [SerializeField] InputActionReference shootAction;
    [SerializeField] InputActionReference reloadAction;
    [SerializeField] InputActionReference Weapon1;
    [SerializeField] InputActionReference Weapon2;
    [SerializeField] InputActionReference Weapon3;
    //[SerializeField] InputActionReference mWheel; // Couldn't figure this out lol
    Vector3 moveDir;
    Vector3 playerVel;

    //public AudioSource jumpSound1;
    //public AudioSource jumpSound2;
    //public AudioSource jumpSound3;

    float hurtSoundTimer;

    public event Action<Weapon> OnWeaponChanged;
    public event Action<int> OnHPChanged;

    void OnEnable()
    {
        moveAction.action?.Enable();
        jumpAction.action?.Enable();
        sprintAction.action?.Enable();
        dashAction.action?.Enable();
        shootAction.action?.Enable();
        Weapon1.action?.Enable();
        Weapon2.action?.Enable();
        Weapon3.action?.Enable();
    }

    void OnDisable()
    {
        moveAction.action?.Disable();
        jumpAction.action?.Disable();
        sprintAction.action?.Disable();
        dashAction.action?.Disable();
        shootAction.action?.Disable();
        Weapon1.action?.Disable();
        Weapon2.action?.Disable();
        Weapon3.action?.Disable();

    }

    void Start()
    {
        HPOrig = HP;
        spawnPlayer();

        for (int i = 0; i < weapons.Count; i++)
        {
            Weapon newWeapon = Instantiate(weapons[i], weaponHolder.transform);

            newWeapon.transform.localPosition = Vector3.zero;
            newWeapon.transform.localRotation = Quaternion.identity;
            newWeapon.transform.localScale = Vector3.one;
            newWeapon.gameObject.SetActive(false);

            weaponModels.Add(newWeapon);
        }

        if (weaponModels.Count > 0)
        {
            currentWeaponIndex = -1;
            SwitchWeapon(0);
        }
        
        OnHPChanged?.Invoke(HP);
        hurtSoundTimer = 5f;
    }

    void Update()
    {
        if (weaponModels.Count > 0 && (currentWeaponIndex == 0 || currentWeaponIndex < weaponModels.Count))
            Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * currentWeapon.data.shootDist, Color.yellow);


        HandleDashInput();
        HandleWeaponSwitch();
        UpdateTimers();
        movement();
        //if (currentWeapon != null && shootAction.action.IsPressed() && shootTimer >= currentWeapon.data.shootRate && !gamemanager.instance.isPaused && !currentWeapon.isReloading)
        if (currentWeapon != null && shootAction.action.IsPressed() && shootTimer >= currentWeapon.data.shootRate && !gamemanager.instance.isPaused && ((!currentWeapon.data.isReloading || currentWeapon.data.isSingleShellReload) && currentWeapon.data.canShootShotgun))
        {
            Debug.Log("Shooting");
            shoot();
        }

        if(currentWeapon != null && reloadAction.action.WasPressedThisFrame())
        {
            if (!currentWeapon.data.isReloading && currentWeapon.canReload())
            {
                //StartCoroutine(currentWeapon.Reload());
                currentWeapon.StartReload();
            }
        }
    }

    public void spawnPlayer()
    {
        controller.transform.position = gamemanager.instance.playerSpawnPos.transform.position;
        Physics.SyncTransforms();
        HP = HPOrig;
    }

    void UpdateTimers()
    {
        shootTimer += Time.deltaTime;

        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
        if (jumpBufferTimer > 0)
        {
            jumpBufferTimer -= Time.deltaTime;
        }
        if (coyoteTimer > 0)
        {
            coyoteTimer -= Time.deltaTime;
        }
        if (hurtSoundTimer > 0)
        {
            hurtSoundTimer -= Time.deltaTime;
        }
    }

    void HandleDashInput()
    {
        //if (Input.GetButtonDown("Dash") && CanDash())
        //{
        //    StartDash();
        //}
        if (dashAction.action.WasPressedThisFrame() && CanDash())
        {
            StartDash();
        }
    }

    bool CanDash()
    {
        return !isDashing && dashCooldownTimer <= 0f;
    }

    void StartDash()
    {
        dashDir = Camera.main.transform.forward.normalized;

        isDashing = true;
        dashTimer = dashTime;
        dashCooldownTimer = dashCooldown;

        playerVel = new Vector3(dashDir.x * dashSpeed, playerVel.y, dashDir.z * dashSpeed);
    }

    void Accelerate(Vector3 wishDir, float wishSpeed, float accel) // Based off Quake movement for bhopping
    {
        Vector3 currentHorizontalVel = new Vector3(playerVel.x, 0, playerVel.z);
        float currentSpeed = Vector3.Dot(currentHorizontalVel, wishDir);
        
        float addSpeed = wishSpeed - currentSpeed;
        if (addSpeed <= 0)
        {
            return;
        }

        float accelSpeed = accel * Time.deltaTime * wishSpeed;
        if (accelSpeed > addSpeed) accelSpeed = addSpeed;

        playerVel.x += wishDir.x * accelSpeed;
        playerVel.z += wishDir.z * accelSpeed;
    }

    void ApplyFriction()
    {
        Vector3 currentHorizontalVel = new Vector3(playerVel.x, 0, playerVel.z);
        float currentSpeed = currentHorizontalVel.magnitude;

        if(currentSpeed < 0.1f)
        {
            playerVel.x = 0;
            playerVel.z = 0;
            return;
        }

        float drop = currentSpeed * groundFriction * Time.deltaTime;
        float newSpeed = currentSpeed - drop;
        if (newSpeed < 0) newSpeed = 0;
        newSpeed /= currentSpeed;

        playerVel.x *= newSpeed;
        playerVel.z *= newSpeed;

    }

    void ApplyHorizontalMovement()
    {
        if (!isDashing)
        {

            Vector3 wishDir = moveDir.normalized;

            float currentMoveSpeed = speed;
            if (sprintAction.action.IsPressed() && controller.isGrounded)
            {
                currentMoveSpeed = speed * sprintMod; // Old sprint code kept slowly increasing movement speed over time
            }

            if (controller.isGrounded && jumpCount == 0)
            {
                ApplyFriction();
                Accelerate(wishDir, currentMoveSpeed, acceleration);
            }
            else
            {
                Accelerate(wishDir, airSpeedCap, airAcceleration);
            }

                //controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else
        {
            //controller.Move(dashDir.normalized * dashSpeed * Time.deltaTime);

            playerVel.x = dashDir.x * dashSpeed;
            playerVel.z = dashDir.z * dashSpeed;

            dashTimer -= Time.deltaTime;

            if(dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
    }

    void movement()
    {

        if (controller.isGrounded)
        {
            jumpCount = 0;
            coyoteTimer = coyoteTime;
            if (playerVel.y < 0)
                playerVel.y = -2f;
        }
        // moveDir = new Vector3(Input.GetAxis("Horizontal"),0, Input.GetAxis("Vertical")); // This works for top down games, but not first person. This movement is global based so gets weird when player rotates
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        moveDir = moveInput.x * transform.right + moveInput.y * transform.forward;
        // moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;

        if (!controller.isGrounded)
            playerVel.y -= gravity * Time.deltaTime;

        jump();
        ApplyHorizontalMovement();

        
        controller.Move(playerVel * Time.deltaTime);
        
    }


    void jump()
    {
        //jump jump
        if(jumpAction.action.WasPressedThisFrame() && jumpCount < jumpMax)
        {
            jumpBufferTimer = jumpBuffer;
            //playerVel.y = jumpSpeed;
            //jumpCount++;
        }
        //if(jumpBufferTimer > 0 && controller.isGrounded) // only works for single jump
        if(jumpBufferTimer > 0 && coyoteTimer > 0) // only works for single jump
        {
            int rand = UnityEngine.Random.Range(1, 7);
            switch (rand)
            {
                case 1:
                    SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.playerJump1);
                    break;
                case 2:
                    SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.playerJump2);
                    break;
                case 3:
                    SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.playerJump3);
                    break;
                default:
                    break;
            }
                
            playerVel.y = jumpSpeed;
            jumpCount = 1;
            jumpBufferTimer = 0;
            coyoteTimer = 0;
        }
    }

    void HandleWeaponSwitch()
    {
        if (Weapon1.action != null && Weapon1.action.WasPressedThisFrame() && weaponModels.Count > 0 && currentWeaponIndex != 0)
        {
            //lastWeapon = currentWeapon; // Handled in SwitchWeapon(int)
            //currentWeaponIndex = 0;
            SwitchWeapon(0);
        }
        else if (Weapon2.action != null && Weapon2.action.WasPressedThisFrame() && weaponModels.Count > 1 && currentWeaponIndex != 1)
        {
            //lastWeapon = currentWeapon; // Handled in SwitchWeapon(int)
            //currentWeaponIndex = 1;
            SwitchWeapon(1);
        }
        else if (Weapon3.action != null && Weapon3.action.WasPressedThisFrame() && weaponModels.Count > 2 && currentWeaponIndex != 2)
        {
            //lastWeapon = currentWeapon; // Handled in SwitchWeapon(int)
            //currentWeaponIndex = 2;
            SwitchWeapon(2);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0 && currentWeaponIndex < weaponModels.Count - 1)
        {
            SwitchWeapon(currentWeaponIndex + 1);
            
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && currentWeaponIndex > 0)
        {
            SwitchWeapon(currentWeaponIndex - 1);
        }
    }

    void SwitchWeapon(int newWeaponIndex)
    {
        if (newWeaponIndex == currentWeaponIndex) return;

        for (int i = 0; i < weaponModels.Count; i++)
        {
            weaponModels[i].gameObject.SetActive(false);
        }
        //weapons[currentWeaponIndex].gameObject.SetActive(false);

        lastWeapon = currentWeapon;
        currentWeaponIndex = newWeaponIndex;
        currentWeapon = weaponModels[newWeaponIndex];

        currentWeapon.gameObject.SetActive(true);

        Debug.Log("Switched to " + currentWeapon.data.weaponName);
        OnWeaponChanged?.Invoke(currentWeapon);

    }

    void shoot()
    {
        if (weaponModels.Count == 0 || currentWeaponIndex < 0 || currentWeaponIndex >= weaponModels.Count)
        {
            Debug.Log("No weapon selected! -- shoot()");
            return;
        }
        shootTimer = 0;

        if(currentWeapon != null)
        {
            if(currentWeapon.canShoot())
                currentWeapon.FireWeapon();
            else
            {
                currentWeapon.GunClick();
            }
        }
        
        //RaycastHit hit;
        //if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, currentWeapon.shootDist, ~ignoreLayer))
        //{
        //    Debug.Log(hit.collider.name);

        //    IDamage dmg = hit.collider.GetComponent<IDamage>();
        //    if(dmg != null)
        //    {
        //        dmg.takeDamage(currentWeapon.shootDamage);
        //    }
        //}
    }

    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
    public Weapon GetLastWeapon()
    {
        return lastWeapon;
    }

    public int GetCurrentHP()
    {
        return HP;
    }
    public int GetMaxHP()
    {
        return HPOrig;
    }

    public void takeDamage(int amount)
    {
        HP -= amount; // do NOT destroy your player
        OnHPChanged?.Invoke(HP);
        int rand = UnityEngine.Random.Range(1, 7);
        if(hurtSoundTimer <= 0) {
            switch (rand)
            {
                case 1:
                    SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.playerHurt1);
                    break;
                case 2:
                    SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.playerHurt2);
                    break;
                case 3:
                    SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.playerHurt3);
                    break;
                case 4:
                    SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.playerHurt4);
                    break;
                case 5:
                    SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.playerHurt5);
                    break;
                case 6:
                    SoundManager.Instance.PlayWithRandomPitch(SoundManager.Instance.playerHurt6);
                    break;
                default:
                    break;
            }
        }
        StartCoroutine(FlashDamage());

        if (HP <= 0)
        {
            // Congrat u r ded
            gamemanager.instance.youLose();
        }
    }

    IEnumerator FlashDamage()
    {
        gamemanager.instance.playerDamageFlashScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gamemanager.instance.playerDamageFlashScreen.SetActive(false);
    }
    public void getWeaponData(WeaponData weapon)
    {
        Weapon newWeapon = Instantiate(weapon.prefab.GetComponent<Weapon>(), weaponHolder.transform); // This is so stupid

        newWeapon.transform.localPosition = Vector3.zero;
        newWeapon.transform.localRotation = Quaternion.identity;
        newWeapon.transform.localScale = Vector3.one;
        newWeapon.gameObject.SetActive(false);

        weaponModels.Add(newWeapon);
        SwitchWeapon(weaponModels.Count - 1);
    }
}
