using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Components
    private Rigidbody rb;

    //Movement Values
    [SerializeField] private float runSpeed = 300f;
    [SerializeField] private float walkSpeed = 100f;
    [SerializeField] private float jumpForce = 100f;

    //Utility
    private float horizontal = 0;
    private float vertical = 0;
    [SerializeField] private LayerMask groundLayer;

    //States
    private bool isJumping = false;
    private bool isWalking = false;
    // Start is called before the first frame update
    void Start()
    {
        //Grab Rigidbody from the object
        rb = GetComponent<Rigidbody>();

        //Turn of main camera
        Camera.main.enabled = false;
    }

    void Update()
    {
        //Get the inputs for vertical and horizontal
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, .2f, groundLayer);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            isJumping = true;
        if (Input.GetKey(KeyCode.LeftShift))
            isWalking = true;
        else
            isWalking = false;
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
}
