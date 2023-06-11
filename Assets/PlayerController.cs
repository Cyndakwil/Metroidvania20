using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Vector2 moveDir;
    private float xAxis;

    //Movement variables
     [Header("Horizontal Movement Settings:")]
    [SerializeField] private float walkSpeed = 20;
    [SerializeField] private float jumpForce = 10f;

    [Header("Ground Check Settings:")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;

    //References
    Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
        GetInput();
        Move();
        Jump();
    }

    void GetInput()
    {
        xAxis = Input.GetAxisRaw("Horizontal");

    }

    void Move()
    {
        rb.velocity = new Vector2(walkSpeed * xAxis, rb.velocity.y);
    }

    public bool Grounded()
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)|| Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround) || Physics2D.Raycast(groundCheckPoint.position + new Vector3(- groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else 
        {
            return false;
        }
    }
    void Jump()
    {
        //Allows player to control the jump.
        if (Input.GetButtonUp("Jump")&& rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }

        //Checks if the player is touching the ground to jump.
        if (Input.GetButtonDown("Jump")&&Grounded())
        {
            rb.velocity = new Vector3(rb.velocity.x,jumpForce);
        }
    }
}
