using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    //Components
    private Rigidbody rb;
    public Transform weaponParent;

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
    #endregion

    #region Monobehavior Callbacks
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        weaponParentOrigin = weaponParent.localPosition;

        //Turn of main camera
        Camera.main.enabled = false;
    }

    void Update()
    {
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
        
            
    }

    // Update is called once per frame
    void FixedUpdate()
    {
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
        targetWeaponBobPos = weaponParentOrigin + new Vector3(Mathf.Cos(px) * pxIntensity, Mathf.Sin(px * 2) * pyIntensity, 0);
    }
    #endregion
}
