using UnityEngine;

public class playerController : MonoBehaviour
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Range(1, 10)] [SerializeField] int HP;
    [Range(3, 7)] [SerializeField] int speed;
    [Range(2, 5)] [SerializeField] int sprintMod;
    [Range(5, 25)] [SerializeField] int jumpSpeed;
    [Range(1, 3)] [SerializeField] int jumpMax;
    [Range(15, 50)] [SerializeField] int gravity;

    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;

    int jumpCount;
    int HPOrig;

    float shootTimer;

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
        movement();
        sprint();
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
        controller.Move(moveDir * speed * Time.deltaTime);

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
}
