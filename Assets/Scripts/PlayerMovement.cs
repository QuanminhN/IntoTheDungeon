using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class PlayerMovement : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Variables
    //Components
    private Rigidbody rb;
    public Transform weaponParent;
    public GameObject cameraParent;

    public Camera normalCam;
    public Camera zoomInCam;
    float baseFOV;

    private Vector3 camOrigin;
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
    private Vector3 WeaponParentCurrentPos;

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
    private Text ui_username;

    //Crouching
    public float crouchModifer;
    public float crouchAmount;
    public GameObject standingCollider;
    public GameObject crouchingCollider;
    private bool crouched;
    private bool callCrouchOnce = true; // This is to call RPC once and not spam the server

    private float aimAngle;
    private bool isAiming;

    [HideInInspector]public PlayerData playerProfile;
    public TextMeshPro playerUsername;
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
            standingCollider.layer = 9;
            crouchingCollider.layer = 9;
        }
        rb = GetComponent<Rigidbody>();
        WeaponParentCurrentPos = weaponParentOrigin = weaponParent.localPosition;

        //Turn off main camera
        if(Camera.main)
            Camera.main.enabled = false;

        //See if it is the correct camera
        cameraParent.SetActive(photonView.IsMine);

        if (photonView.IsMine)
        {
            camOrigin = normalCam.transform.localPosition;
            ui_healthBar = GameObject.Find("HUD/Health/Bar").transform;
            ui_ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();
            ui_username = GameObject.Find("HUD/Username/Text").GetComponent<Text>();
            Current_health = Max_health;
            baseFOV = normalCam.fieldOfView;
            ui_username.text = Launcher.myProfile.username;
            updateHealthBar();
            photonView.RPC("SyncUserProfile", RpcTarget.All, Launcher.myProfile.username, Launcher.myProfile.level, Launcher.myProfile.xp);
        }
    }

    void Update()
    {
        if (!photonView.IsMine) //If the view does not belong to the player you can move them
        {
            RefreshMultiplayerState();
            return;
        }
        //Get the inputs for vertical and horizontal
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        //States
        bool isGrounded = Physics.Raycast(groundChecker.position, Vector3.down, .2f, groundLayer);
        bool crouch = Input.GetKey(KeyCode.LeftControl);
        bool pause = Input.GetKeyDown(KeyCode.Escape);
        isAiming = Input.GetMouseButton(1);

        //Pausing
        if (pause)
        {
            GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
        }
        if (Pause.paused)
        {
            horizontal = 0f;
            vertical = 0f;
            isGrounded = false;
            isJumping = false;
            crouch = false;
            isWalking = false;
        }

        //Crouching
        if (crouch)
        {
            if(!crouched && callCrouchOnce)
                photonView.RPC("SetCrouch", RpcTarget.All, true);
        }
        else
        {
            if(crouched && !callCrouchOnce)
                photonView.RPC("SetCrouch", RpcTarget.All, false);
        }

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
        else if (crouched)
        {
            HeadBob(movementCounter, .02f, .02f);
            idleCounter += Time.deltaTime * 1.75f;
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

        if (Input.GetKeyDown(KeyCode.U)) TakeDamage(100, -1);

        weapon.Aim(isAiming);

        if (isAiming)
        {
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * weapon.currentGunData.MainFOV, Time.deltaTime * 8f);
            zoomInCam.fieldOfView = Mathf.Lerp(zoomInCam.fieldOfView, baseFOV * weapon.currentGunData.ZoomInFOV, Time.deltaTime * 8f);
        }
        else
        {
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
            zoomInCam.fieldOfView = Mathf.Lerp(zoomInCam.fieldOfView, baseFOV, Time.deltaTime * 8f);
        }

        //Smooth out UI 
        updateHealthBar();
        weapon.refreshAmmo(ui_ammo);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!photonView.IsMine) return; //If the view does not belong to the player you can move them

        float speed = runSpeed;
        if (Pause.paused)
        {
            speed = 0;
            horizontal = 0;
            vertical = 0;
            isJumping = false;
            isWalking = false;
            crouched = false;
        }
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
        else if (crouched)
            speed *= crouchModifer;

        Vector3 t_speed = transform.TransformDirection(t_move) * speed * Time.deltaTime;
        t_speed.y = rb.velocity.y;
        rb.velocity = t_speed;

        //Camera modifier
        if (crouched)
        {
            normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition,
            camOrigin + Vector3.down * crouchAmount, Time.deltaTime * 6f);
        }
        else
        {
            normalCam.transform.localPosition = Vector3.Lerp(normalCam.transform.localPosition,
            camOrigin, Time.deltaTime * 6f);
        }
        
    }
    #endregion

    #region Private Methods
    void HeadBob(float px, float pxIntensity, float pyIntensity)
    {
        float t_aimAdj = 1f;
        if (weapon.isAiming) t_aimAdj = .1f;
        targetWeaponBobPos = WeaponParentCurrentPos + new Vector3(Mathf.Cos(px) * pxIntensity * t_aimAdj, Mathf.Sin(px * 2) * pyIntensity * t_aimAdj, 0);
    }

    [PunRPC]
    void SetCrouch(bool t_state)
    {
        if (crouched == t_state) return;

        crouched = t_state;

        callCrouchOnce = !callCrouchOnce;
        if (crouched)
        {
            standingCollider.SetActive(false);
            crouchingCollider.SetActive(true);
            WeaponParentCurrentPos += Vector3.down * crouchAmount;
        }
        else
        {
            standingCollider.SetActive(true);
            crouchingCollider.SetActive(false);
            WeaponParentCurrentPos -= Vector3.down * crouchAmount;
        }
    }

    private void RefreshMultiplayerState()
    {
        // Keep track of where the other players are looking
        // and set it so that it is shown to the network
        float cacheEulY = weaponParent.localEulerAngles.y;
        Quaternion targetRotation = Quaternion.identity * Quaternion.AngleAxis(aimAngle, Vector3.right);
        weaponParent.rotation = Quaternion.Slerp(weaponParent.rotation, targetRotation, Time.deltaTime * 8f);

        Vector3 finalRotation = weaponParent.localEulerAngles;
        finalRotation.y = cacheEulY;

        weaponParent.localEulerAngles = finalRotation;
    }

    [PunRPC]
    void SyncUserProfile(string t_username, int t_level, int t_xp)
    {
        playerProfile = new PlayerData(t_username, t_level, t_xp);
        playerUsername.text = playerProfile.username;
    }
    #endregion

    #region Public Methods
    public void TakeDamage(int dmg, int t_actor)
    {
        if (photonView.IsMine)
        {
            
            //Deal damage
            Current_health -= dmg;
            //Update Health HUD
            updateHealthBar();
            //Kill if player has less than 0 health
            if (Current_health <= 0)
            {
                //spawn player after destroying old body
                manager.Spawn();
                //Send to network to update stats
                manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
                Debug.Log("CHANGE STATS FOR :" + PhotonNetwork.LocalPlayer.ActorNumber);
                if (t_actor >= 0)
                    manager.ChangeStat_S(t_actor , 0, 1);
                //Destory object
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

    #region Photon Callback
    public void OnPhotonSerializeView(PhotonStream p_stream, PhotonMessageInfo p_message)
    {
        if (p_stream.IsWriting)//If it is the player write to the stream to send to network
        {
            p_stream.SendNext((int)(weaponParent.transform.localEulerAngles.x * 100f));
        }
        else//not the player so... read the stream
        {
            aimAngle = (int)p_stream.ReceiveNext() / 100f;
        }
    }
    #endregion
}
