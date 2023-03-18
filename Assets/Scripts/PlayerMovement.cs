using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    #region Variables
    //Components
    private Rigidbody rb;
    public Transform weaponParent;
    public GameObject cameraParent;

    //Movement Values
    [SerializeField] private float runSpeed = 300f;
    [SerializeField] private float walkSpeed = 100f;
    [SerializeField] private float jumpForce = 100f;

    //Utility
    private float horizontal = 0;
    private float vertical = 0;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundChecker;

    private Vector3 weaponParentOrigin;
    private Vector3 targetWeaponBobPos;

    private float movementCounter;
    private float idleCounter;

    //States
    private bool isJumping = false;
    private bool isWalking = false;

    //Character Stats
    public float Max_health;
    private float Current_health;

    //Manager
    private GameManager manager;
    private Weapon weapon;

    //UI
    private Transform ui_healthBar;
    private Text ui_ammo;
    #endregion

    #region Monobehavior Callbacks
    // Start is called before the first frame update
    void Start()
    {
        //Find the components
        manager = GameObject.Find("Manager").GetComponent<GameManager>();
        weapon = GetComponent<Weapon>();

        //Change layer if character is not the player
        if (!photonView.IsMine)
        {
            gameObject.layer = 9;
        }
        rb = GetComponent<Rigidbody>();
        weaponParentOrigin = weaponParent.localPosition;

        //Turn off main camera
        if(Camera.main)
            Camera.main.enabled = false;

        //See if it is the correct camera
        cameraParent.SetActive(photonView.IsMine);

        if (photonView.IsMine)
        {
            ui_healthBar = GameObject.Find("HUD/Health/Bar").transform;
            ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
            Current_health = Max_health;
            updateHealthBar();
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return; //If the view does not belong to the player you can move them

        //Get the inputs for vertical and horizontal
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        bool isGrounded = Physics.Raycast(groundChecker.position, Vector3.down, .2f, groundLayer);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            isJumping = true;

        //Check if player is walking
        if (Input.GetKey(KeyCode.LeftShift))
            isWalking = true;
        else
            isWalking = false;

        //Headbob 
        if (horizontal == 0 && vertical == 0) //Idle
        {
            HeadBob(idleCounter, .025f, .025f);
            idleCounter += Time.deltaTime;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPos, Time.deltaTime * 2f);
        }
        else if (isWalking)
        {
            HeadBob(movementCounter, .05f, .05f);
            movementCounter += Time.deltaTime * 3f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPos, Time.deltaTime * 4f);
        }
        else
        {
            HeadBob(movementCounter, .05f, .05f);
            movementCounter += Time.deltaTime * 5f;
            weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPos, Time.deltaTime * 6f);
        }

        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(25);

        //Smooth out UI 
        updateHealthBar();
        weapon.refreshAmmo(ui_ammo);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!photonView.IsMine) return; //If the view does not belong to the player you can move them

        float speed = runSpeed;

        Vector3 t_move = new Vector3(horizontal, 0, vertical);
        t_move.Normalize();
        //Debug.Log(t_move);
        if (isJumping)
        {
            rb.AddForce(Vector3.up * jumpForce);
            isJumping = false;
        }
        if (isWalking)
            speed = walkSpeed;

        Vector3 t_speed = transform.TransformDirection(t_move) * speed * Time.deltaTime;
        t_speed.y = rb.velocity.y;
        rb.velocity = t_speed;
        
    }
    #endregion

    #region Private Methods
    void HeadBob(float px, float pxIntensity, float pyIntensity)
    {
        float t_aimAdj = 1f;
        if (weapon.isAiming) t_aimAdj = .1f;
        targetWeaponBobPos = weaponParentOrigin + new Vector3(Mathf.Cos(px) * pxIntensity * t_aimAdj, Mathf.Sin(px * 2) * pyIntensity * t_aimAdj, 0);
    }
    #endregion

    #region Public Methods
    public void TakeDamage(int dmg)
    {
        if (photonView.IsMine)
        {
            //Deal damage
            Current_health -= dmg;
            //Update Health HUD
            updateHealthBar();
            //Kill if player has 0 health
            if (Current_health <= 0)
            {
                manager.Spawn();
                PhotonNetwork.Destroy(this.gameObject);
            }
            

        }    
    }

    private void updateHealthBar()
    {
        float temp_health = (float)Current_health / (float)Max_health;
        ui_healthBar.localScale = Vector3.Lerp(ui_healthBar.localScale, new Vector3(temp_health, 1, 1), Time.deltaTime * 8f);
    }
    #endregion
}
