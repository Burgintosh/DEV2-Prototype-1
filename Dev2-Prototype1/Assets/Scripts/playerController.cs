using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;


    [Range(1, 10)] [SerializeField] int HP;
    [Range(3, 7)] [SerializeField] float speed;
    [Range(2, 5)] [SerializeField] float sprintMod;
    [Range(5, 25)] [SerializeField] float jumpSpeed;
    [Range(1, 3)] [SerializeField] int jumpMax;
    [Range(15, 50)] [SerializeField] float gravity;

    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;

    [Header("Dash")]
    [SerializeField] float dashSpeed;
    [SerializeField] float dashTime;
    [SerializeField] float dashCooldown;

    [Header("Input Setup (For Input System Package")]
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference jumpAction;
    [SerializeField] InputActionReference sprintAction;
    [SerializeField] InputActionReference dashAction;
    [SerializeField] InputActionReference shootAction;

    [Header("Movement")]
    [Range(50, 200)] [SerializeField] float acceleration = 75f; // Default 75
    [Range(100, 300)] [SerializeField] float airAcceleration = 150f; // Default 150
    [Range(1, 30)] [SerializeField] float groundFriction = 25f; // Default 25
    [Range(0.1f, 0.5f)] [SerializeField] float jumpBuffer = 0.4f; // Default 0.4
    [Range(0f, 10f)] [SerializeField] float airSpeedCap = 1f; // Default 1 (0 for no cap)



    int jumpCount;
    int HPOrig;

    float shootTimer;
    float jumpBufferTimer;

    // Dash
    float dashTimer;
    float dashCooldownTimer;
    bool isDashing;
    Vector3 dashDir;

    Vector3 moveDir;
    Vector3 playerVel;

    void OnEnable()
    {
        moveAction.action?.Enable();
        jumpAction.action?.Enable();
        sprintAction.action?.Enable();
        dashAction.action?.Enable();
        shootAction.action?.Enable();
    }

    void OnDisable()
    {
        moveAction.action?.Disable();
        jumpAction.action?.Disable();
        sprintAction.action?.Disable();
        dashAction.action?.Disable();
        shootAction.action?.Disable();
    }

    void Start()
    {
        HPOrig = HP;
    }

    void Update()
    {
        HandleDashInput();
        UpdateTimers();
        movement();
        sprint();
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

            if (controller.isGrounded && jumpCount == 0)
            {
                ApplyFriction();
                Accelerate(wishDir, speed, acceleration);
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
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.yellow);

        shootTimer += Time.deltaTime;

        if (controller.isGrounded)
        {
            jumpCount = 0;
            if(playerVel.y < 0)
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
       

        //if(Input.GetButton("Fire1") && shootTimer >= shootRate)
        //{
        //    shoot();
        //}
        if(shootAction.action.IsPressed() && shootTimer >= shootRate)
        {
            shoot();
        }
    }

    void sprint()
    {
        //if(Input.GetButtonDown("Sprint")) // GetButton is polling ie hold to sprint, Down and Up are toggles
        //{
        //    speed *= sprintMod;
        //}
        //else if(Input.GetButtonUp("Sprint"))
        //{
        //    speed /= sprintMod;
        //}
        if (sprintAction.action.WasPressedThisFrame()) // GetButton is polling ie hold to sprint, Down and Up are toggles
        {
            speed *= sprintMod;
        }
        else if (sprintAction.action.WasReleasedThisFrame())
        {
            speed /= sprintMod;
        }
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
        if(jumpBufferTimer > 0 && controller.isGrounded) // only works for single jump
        {
            playerVel.y = jumpSpeed;
            jumpCount = 1;
            jumpBufferTimer = 0;
        }
    }

    void shoot()
    {
        shootTimer = 0;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if(dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount; // do NOT destroy your player

        StartCoroutine(damageFlash());
        if (HP <= 0)
        {
            // Congrat u r ded
            gamemanager.instance.youLose();
        }
    }

    IEnumerator damageFlash()
    {
        gamemanager.instance.playerDamageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gamemanager.instance.playerDamageFlash.SetActive(false);
    }
}
