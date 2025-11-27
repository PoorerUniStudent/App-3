using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int speed;
    [SerializeField] private int jumpPower;
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer playerSprite;
    [SerializeField] private Transform groundCheck;

    private PlayerControls playerControls;
    private Rigidbody rb;
    private Vector3 movement;

    private const string IS_WALK_PARAM = "run";
    private const string IS_IDLE_PARAM = "idle";
    private const string IS_JUMPING_PARAM = "jump";
    private const string IS_IN_AIR_PARAM = "inAir";
    private const string IS_LANDING_PARAM = "landing";

    private string currentState = IS_IDLE_PARAM;

    private bool isGrounded = true;
    [SerializeField] private float groundCheckRadius = 0.5f;
    [SerializeField] private LayerMask whatIsGround;

    private const int MAX_JUMPS = 1;
    private int numOfJumps = MAX_JUMPS;
    private bool coyoteTime = false;
    private float coyoteTimeDuration = 0.2f; // in seconds
    private float coyoteTimeStart;

    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float x = playerControls.Player.Move.ReadValue<Vector2>().x;
        float z = playerControls.Player.Move.ReadValue<Vector2>().y;
        bool jump = playerControls.Player.Jump.WasPressedThisFrame();

        CheckIfGrounded();
        CheckCoyoteTime();

        movement = new Vector3(x, 0, z).normalized;

        if (jump && numOfJumps > 0 && (isGrounded || coyoteTime)) 
        {
            numOfJumps -= 1;
            coyoteTime = false;
            currentState = IS_JUMPING_PARAM;
            anim.SetBool(IS_IDLE_PARAM, false);
            anim.SetBool(IS_WALK_PARAM, false);
            anim.SetBool(IS_LANDING_PARAM, false);

            anim.SetBool(IS_JUMPING_PARAM, true);
        }
        else if (!isGrounded)
        {
            if (currentState != IS_IN_AIR_PARAM)
            {
                coyoteTime = true;
                coyoteTimeStart = Time.time;
            }

            currentState = IS_IN_AIR_PARAM;
            anim.SetFloat("yVelocity", rb.velocity.y);
            anim.SetBool(IS_IN_AIR_PARAM, true);

            anim.SetBool(IS_IDLE_PARAM, false);
            anim.SetBool(IS_WALK_PARAM, false);
            anim.SetBool(IS_JUMPING_PARAM, false);
        }
        else if ((isGrounded && currentState == IS_IN_AIR_PARAM) || currentState == IS_LANDING_PARAM)
        {
            currentState = IS_LANDING_PARAM;
            anim.SetBool(IS_LANDING_PARAM, true);

            anim.SetBool(IS_IN_AIR_PARAM, false);

            if (movement != Vector3.zero)
            {
                LandingFinishedTrigger();
            }
        }
        else
        {
            CheckGroundStates();
        }

        if (x < 0)
        {
            playerSprite.flipX = true;
        }
        else if (x > 0)
        {
            playerSprite.flipX = false;
        }
    }

    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + movement * speed * Time.fixedDeltaTime);
    }

    private void CheckIfGrounded()
    {
        if (groundCheck == null) return;
        Collider[] hits = Physics.OverlapSphere(groundCheck.position, groundCheckRadius, whatIsGround);

        if (hits.Length > 0)
        {
            isGrounded = true;
            numOfJumps = MAX_JUMPS;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void CheckCoyoteTime()
    {
        if (Time.time >= coyoteTimeStart + coyoteTimeDuration)
        {
            coyoteTime = false;
        }
    }

    // For debugging
    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.transform.position, groundCheckRadius);
    }

    private void CheckGroundStates()
    {
        if (currentState == IS_JUMPING_PARAM) return;

        if (movement == Vector3.zero)
        {
            currentState = IS_IDLE_PARAM;
            anim.SetBool(IS_IDLE_PARAM, true);

            anim.SetBool(IS_WALK_PARAM, false);
        }
        else
        {
            currentState = IS_WALK_PARAM;
            anim.SetBool(IS_WALK_PARAM, true);

            anim.SetBool(IS_IDLE_PARAM, false);
        }
    }

    private void AddForceY(int force)
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
    }

    public void LandingFinishedTrigger()
    {
        currentState = IS_IDLE_PARAM;
        anim.SetBool(IS_IDLE_PARAM, true);

        anim.SetBool(IS_LANDING_PARAM, false);
    }

    public void JumpTrigger()
    {
        AddForceY(jumpPower);
    }
}


