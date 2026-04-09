using UnityEngine;

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

    int jumpCount;
    int HPOrig;

    float shootTimer;

    // Dash
    float dashTimer;
    float dashCooldownTimer;

    bool isDashing;

    Vector3 dashDir;

    Vector3 moveDir;
    Vector3 playerVel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
    }

    // Update is called once per frame
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
    }

    void HandleDashInput()
    {
        if (Input.GetButtonDown("Dash") && CanDash())
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
        if(moveDir.magnitude > 0)
        {
            dashDir = moveDir.normalized;
        }
        else
        {
            dashDir = transform.forward;
        }

        isDashing = true;
        dashTimer = dashTime;
        dashCooldownTimer = dashCooldown;
    }

    void ApplyHorizontalMovement()
    {
        if (!isDashing)
        {
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
        else
        {
            controller.Move(dashDir.normalized * dashSpeed * Time.deltaTime);

            dashTimer -= Time.deltaTime;

            if(dashTimer < 0f)
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
            if(playerVel.y < 0) // Not needed but I think might make it nicer
                playerVel.y = 0;
        }
        // moveDir = new Vector3(Input.GetAxis("Horizontal"),0, Input.GetAxis("Vertical")); // This works for top down games, but not first person. This movement is global based so gets weird when player rotates
        
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        ApplyHorizontalMovement();

        jump();
        controller.Move(playerVel * Time.deltaTime);
        if(!controller.isGrounded) // Not needed if statement, but stops playerVel from decreasing when on the ground
            playerVel.y -= gravity * Time.deltaTime;

        if(Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
    }

    void sprint()
    {
        if(Input.GetButtonDown("Sprint")) // GetButton is polling ie hold to sprint, Down and Up are toggles
        {
            speed *= sprintMod;
        }
        else if(Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod; // Not sure how this is different than the GetButton to do hold to sprint
        }
    }

    void jump()
    {
        //jump jump
        if(Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    void shoot()
    {
        shootTimer = 0;

        RaycastHit hit; // Basically hitscan
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

        if (HP <= 0)
        {
            // Congrat u r dedaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
            gamemanager.instance.youLose();
        }
    }
}
