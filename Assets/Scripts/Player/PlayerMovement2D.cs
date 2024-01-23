using Photon.Pun;
using System;
using UnityEngine;

public class PlayerMovement2D : MonoBehaviourPun
{
    // Horizontal movement variables
    [Header("Horizontal variables")]
    public float accel;
    public float decel;
    public float airDecel;
    float tempDecel;
    public float maxSpeed;
    public float knockBackHorizontalStrenght;

    // Vertical movement variables
    [Header("Vertical variables")]
    public float jumpStrenght;
    public float jumpBufferLenght;
    public float maxFallSpeed;
    public float wallJumpHorizontal;
    public float wallJumpVertical;
    public float knockBackVerticalStrenght;

    [Header("Gravity variables")]
    public float gravity;
    public float gravZoneMult;
    public float upGravMult;
    public float neutralGravMult;
    public float downGravMult;
    public float upWallGravMult;
    public float downWallGravMult;
    float tempGrav;

    [Header("Walljump variables")]
    public float clingDuration;
    public float maxDownSlideSpeed;
    public float wallJumpBuffer;
    public bool canWallCling = true;
    public bool canWallJump = true;
    float jumpBuffer;
    float onLeftWallCling;
    float onRightWallCling;
    float wallJumpBufferL;
    float wallJumpBufferR;

    // Collisions
    [Header("Collision")]
    public float colisionDistance;
    public LayerMask sludgeMask;
    public LayerMask sideMask;
    bool grounded;
    bool leftCol;
    bool rightCol;

    // Input
    bool upPressed;
    bool upHold;
    // Commented out to remove unused warning
    //bool leftPressed;
    bool leftHold;
    // Commented out to remove unused warning
    //bool rightPressed;
    bool rightHold;

    int countWallClingCollision;
    int countWallClingGrounded;

    // Crushing variables
    public ContactFilter2D crushFilter;
    [Range(0.0f, 1.0f)]
    public float crushHitboxMult;
    int countCrushing;
    Collider2D[] crushResults = new Collider2D[10];

    PlayerManager playerManager;
    Vector3 lastSpeed;

    // Global variables
    Vector3 moveSpeed;
    Rigidbody2D rb;

    private PlayerRender render;
    private bool hasLanded = false;
    int rotation = 0;
    public bool justLandedLocally = false;

    // Start is called before the first frame update
    void Start()
    {
        // Disable script if player is not the local player.
        if (photonView != null && !photonView.IsMine) { enabled = false; }

        // Get the rigidbody
        rb = GetComponent<Rigidbody2D>();
        // Get the player status effects script
        playerManager = GetComponent<PlayerManager>();

        render = GetComponentInChildren<PlayerRender>();

        // Reset movespeed on start
        moveSpeed = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        InputCheck();
    }

    // FixedUpdate is called at a fixed interval
    void FixedUpdate()
    {


        // Check for colisions
        CheckColision();
        // Change horizontal movement
        HorizontalMove();
        // Change vertical movement
        VerticalMove();
        // Pressed should always be in effect one fixedUpdate after keydown
        ResetPressed();
    }

    // This function checks the relevant inputs
    void InputCheck()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
        {
            upPressed = true;
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
        {
            upHold = true;
        }
        else
        {
            upHold = false;
        }
        /* Commented out to remove unused warning
        if (Input.GetKeyDown(KeyCode.A))
        {
            leftPressed = true;
        }
        */
        if (Input.GetKey(KeyCode.A))
        {
            leftHold = true;
        }
        else
        {
            leftHold = false;
        }
        /* Commented out to remove unused warning
        if (Input.GetKeyDown(KeyCode.D))
        {
            rightPressed = true;
        }
        */
        if (Input.GetKey(KeyCode.D))
        {
            rightHold = true;
        }
        else
        {
            rightHold = false;
        }

    }

    // To use keyDown with FixedUpdate it should reset only after one FixedUpdate
    void ResetPressed()
    {
        upPressed = false;
        // Commented out to remove unused warning
        //leftPressed = false;
        //rightPressed = false;
    }

    // This function handles horizontal movement
    void HorizontalMove()
    {
        // Horizontal movement
        // Get the current velocity to prevent clipping
        moveSpeed = rb.velocity;
        // Left (and not right input) for moving left
        if (leftHold && !rightHold)
        {
            // When currently moving the other way add the decel to decrease slip
            if (moveSpeed.x > 0)
            {
                // Prevent moving back to a wall too quickly
                if (wallJumpBufferL > 0)
                {
                    moveSpeed.x -= accel * Time.fixedDeltaTime * 0.5f;
                }
                else
                {
                    moveSpeed.x -= (accel + decel) * Time.fixedDeltaTime;
                }
            }
            // Accelerate left
            else
            {
                if (!(onRightWallCling > 0))
                {
                    moveSpeed.x -= accel * Time.fixedDeltaTime;
                }
                else
                {
                    onRightWallCling -= Time.fixedDeltaTime;
                }
            }
        }
        // Right (and not right input) for moving right
        else if (rightHold && !leftHold)
        {
            // When currently moving the other way add the decel to decrease slip
            if (moveSpeed.x < 0)
            {
                // Prevent moving back to a wall too quickly
                if (wallJumpBufferR > 0)
                {
                    moveSpeed.x += accel * Time.fixedDeltaTime * 0.5f;
                }
                else
                {
                    moveSpeed.x += (accel + decel) * Time.fixedDeltaTime;
                }
            }
            // Accelerate right
            else
            {
                if (!(onLeftWallCling > 0))
                {
                    moveSpeed.x += accel * Time.fixedDeltaTime;
                }
                else
                {
                    onLeftWallCling -= Time.fixedDeltaTime;
                }
            }
        }
        // No horizontal input or both decelerate
        else
        {
            if (onLeftWallCling > 0)
            {
                onLeftWallCling = clingDuration;
            }
            if (onRightWallCling > 0)
            {
                onRightWallCling = clingDuration;
            }

            // The temporary instance of deceleration
            tempDecel = decel;

            // Have a different decelaretion while in the air
            if (!grounded)
            {
                tempDecel = airDecel;
            }
            // Decelerate
            if (moveSpeed.x >= tempDecel * Time.fixedDeltaTime)
            {
                moveSpeed.x -= tempDecel * Time.fixedDeltaTime;
            }
            if (moveSpeed.x <= -tempDecel * Time.fixedDeltaTime)
            {
                moveSpeed.x += tempDecel * Time.fixedDeltaTime;
            }
            // When current speed is lower then the decel amount set speed to 0
            if (moveSpeed.x > -tempDecel * Time.fixedDeltaTime && moveSpeed.x < tempDecel * Time.fixedDeltaTime)
            {
                moveSpeed.x = 0;
            }
        }

        // Keep speed within max speed bounds
        if (moveSpeed.x > maxSpeed)
        {
            moveSpeed.x = maxSpeed;
        }
        if (moveSpeed.x < -maxSpeed)
        {
            moveSpeed.x = -maxSpeed;
        }

        // Handle the rotation of the player based on current x speed
        HandleRotation(moveSpeed);
        // Apply new speed
        rb.velocity = moveSpeed;
    }

    // This fuction rotates the player sprite depending on movement direction
    private void HandleRotation(Vector3 moveSpeed)
    {
        if (moveSpeed.x > 0.1f) rotation = 0;
        else if (moveSpeed.x < -0.1f) rotation = 180;
        render.transform.rotation = Quaternion.Euler(0, rotation, 0);
    }

    // This funciton handles vertical movement
    void VerticalMove()
    {
        // Get the current velocity to edit
        moveSpeed = rb.velocity;

        // When you try to jump create a buffer for better feel
        if (upPressed)
        {
            jumpBuffer = jumpBufferLenght;
        }

        // Decrease wallJump buffer
        if (wallJumpBufferL > 0)
        {
            // No buffer when on the ground
            if (grounded)
            {
                wallJumpBufferL = 0;
            }
            else
            {
                wallJumpBufferL -= Time.fixedDeltaTime;
            }

        }
        if (wallJumpBufferR > 0)
        {
            // No buffer when on the ground
            if (grounded)
            {
                wallJumpBufferR = 0;
            }
            else
            {
                wallJumpBufferR -= Time.fixedDeltaTime;
            }
        }

        // After inputing jump
        if (jumpBuffer > 0)
        {
            // Lower jumpbuffer value overtime
            jumpBuffer -= Time.fixedDeltaTime;

            // When you are on the ground and want to jump
            if (grounded && jumpBuffer > 0)
            {
                jumpBuffer = 0;
                moveSpeed.y = jumpStrenght;

                // Play one shot in FMOD of jump sound
                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Jump");

                hasLanded = false;
                render.Jump();
            }
            // When you cling onto a wall do a walljump
            if (onLeftWallCling > 0 && canWallJump && jumpBuffer > 0)
            {
                jumpBuffer = 0;
                moveSpeed.x = wallJumpHorizontal;
                moveSpeed.y = wallJumpVertical;
                wallJumpBufferL = wallJumpBuffer;
                onLeftWallCling = 0;
                onRightWallCling = 0;

                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Jump");
                render.Jump();
            }
            // When you cling onto a wall do a walljump
            if (onRightWallCling > 0 && canWallJump && jumpBuffer > 0)
            {
                jumpBuffer = 0;
                moveSpeed.x = -wallJumpHorizontal;
                moveSpeed.y = wallJumpVertical;
                wallJumpBufferR = wallJumpBuffer;
                onRightWallCling = 0;
                onLeftWallCling = 0;

                FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Jump");
                render.Jump();
            }
        }

        // When in the air apply gravity
        if (!grounded)
        {
            tempGrav = gravity;

            // Apply zone gravity multiplier
            tempGrav *= gravZoneMult;

            // When clinging on wall lower gravity
            if (onLeftWallCling > 0 || onRightWallCling > 0)
            {
                if (moveSpeed.y > 0)
                {
                    tempGrav *= upWallGravMult;
                }
                else
                {
                    tempGrav *= downWallGravMult;
                }
            }

            // When moving up
            if (moveSpeed.y > 0)
            {
                // Create jump variance based on holding jump
                if (upHold)
                {
                    // Have a lower gravity when holding up to create short and long hops
                    moveSpeed.y -= tempGrav * upGravMult * Time.fixedDeltaTime;
                }
                else
                {
                    // Have a neutral gravity between falling and holding up
                    moveSpeed.y -= tempGrav * neutralGravMult * Time.fixedDeltaTime;
                }
            }
            // When falling
            if (moveSpeed.y <= 0)
            {
                // Have a higher gravity
                moveSpeed.y -= tempGrav * downGravMult * Time.fixedDeltaTime;
            }
        }

        // Have a maximum speed to slide down walls
        if (onLeftWallCling > 0 || onRightWallCling > 0)
        {
            // Have a maximum speed at which you can slide down a wall
            if (moveSpeed.y < -maxDownSlideSpeed)
            {
                moveSpeed.y = -maxDownSlideSpeed;
            }
        }

        // Have a maximum fallspeed
        if (moveSpeed.y < -maxFallSpeed)
        {
            moveSpeed.y = -maxFallSpeed;
        }

        // Apply movement
        rb.velocity = moveSpeed;
    }

    // Check for collisions using boxcast
    void CheckColision()
    {
        // Check left for collision
        if (Physics2D.BoxCast(transform.position, transform.localScale, 0, Vector2.left, colisionDistance, sideMask))
        {
            leftCol = true;
        }
        else
        {
            leftCol = false;
        }
        // Check right for collision
        if (Physics2D.BoxCast(transform.position, transform.localScale, 0, Vector2.right, colisionDistance, sideMask))
        {
            rightCol = true;
        }
        else
        {
            rightCol = false;
        }

        // Check down for collision
        if (Physics2D.BoxCast(transform.position, transform.localScale, 0, Vector2.down, colisionDistance, sludgeMask))
        {
            grounded = true;
            if (!hasLanded)
            {
                justLandedLocally = true;
                render.Land();
                hasLanded = true;
            }
        }
        else
        {
            grounded = false;
        }

        // Check if the player is getting crushed
        CrushTest();

        // If the player doesn't detect collision with the wall it's clinging on for more than 5 updates in a row, the player is no longer clinging to the wall
        if (onLeftWallCling > 0)
        {
            // Check if there still is a wall to the left
            if (Physics2D.BoxCast(transform.position, transform.localScale, 0, Vector2.left, colisionDistance, sideMask) && canWallCling)
            {
                // Reset the counter
                countWallClingCollision = 0;
            }
            else
            {
                // Count the number of updates not detecting the wall
                countWallClingCollision++;
                // When the wall hasn't been detected for more than 5 frames
                if (countWallClingCollision >= 5)
                {
                    // Disable wall cling
                    onLeftWallCling = 0;
                }
            }

        }
        if (onRightWallCling > 0)
        {
            // Check if there still is a wall to the right
            if (Physics2D.BoxCast(transform.position, transform.localScale, 0, Vector2.right, colisionDistance, sideMask) && canWallCling)
            {
                // Reset the counter
                countWallClingCollision = 0;
            }
            else
            {
                // Count the number of updates not detecting the wall
                countWallClingCollision++;
                // When the wall hasn't been detected for more than 5 frames
                if (countWallClingCollision >= 5)
                {
                    // Disable wall cling
                    onRightWallCling = 0;
                }
            }
        }

        // When moving into the right wall or when against the right wall holding right start wallclinging
        if ((rightCol && lastSpeed.x > 0) || (rightCol && rightHold) && canWallCling)
        {
            // Start wall clinging on the right wall
            onRightWallCling = clingDuration;
            lastSpeed.x = 0;
        }
        // When moving into the left wall or when against the right wall holding right start wallclinging
        if ((leftCol && lastSpeed.x < 0) || (leftCol && leftHold) && canWallCling)
        {
            // Start wall clinging on the left wall
            onLeftWallCling = clingDuration;
            lastSpeed.x = 0;
        }

        // Reset wall cling when on the ground
        if (grounded && (onLeftWallCling > 0 || onRightWallCling > 0))
        {
            countWallClingGrounded++;
            if (countWallClingGrounded > 3)
            {
                onLeftWallCling = 0;
                onRightWallCling = 0;
            }
        }
        else
        {
            countWallClingGrounded = 0;
        }

        if (Physics2D.BoxCast(transform.position, transform.localScale, 0, Vector2.down, 0.05f, sludgeMask))
        {
            // Check if falling platform is below the player and if it is parent it to it.s
            RaycastHit2D downHit = Physics2D.BoxCast(transform.position, transform.localScale, 0, Vector2.down, 0.05f, sludgeMask);
            if (transform.parent == null && downHit.transform.tag == "Falling Platform")
            {
                transform.parent = downHit.transform;
            }
        }
        else
        {
            transform.parent = null;
        }

        // When clinging on a wall horizontal movement shouldn't be possible, so when horizontal movement in detected reset wall cling
        if (onLeftWallCling > 0 || onRightWallCling > 0)
        {
            // Horizontal movemenent is a significant amount
            if (rb.velocity.x > 0.1 || rb.velocity.x < -0.1)
            {
                // Reset both wall clings
                onLeftWallCling = 0;
                onRightWallCling = 0;
            }
        }

        // Keep track of fast movement so long
        if (rb.velocity.x > 0.1 || rb.velocity.x < -0.1 || grounded)
        {
            lastSpeed = rb.velocity;
        }
    }

    public void UnParent()
    {
        // Unparent the object
        transform.parent = null;
    }

    public void KnockBack()
    {
        moveSpeed = rb.velocity;
        moveSpeed.y = knockBackVerticalStrenght;

        if (knockBackHorizontalStrenght != 0)
        {
            if (moveSpeed.x > 0)
            {
                moveSpeed.x = -knockBackHorizontalStrenght;
            }
            else if (moveSpeed.x < 0)
            {
                moveSpeed.x = knockBackHorizontalStrenght;
            }
        }

        rb.velocity = moveSpeed;
    }

    // This function tests if the player hitbox is overlapping with more than 1 other hitbox and "crushes" him when this is the case for more than 5 updates
    void CrushTest()
    {
        // Check for crush with box overlap
        if (Physics2D.OverlapBox(transform.position, transform.localScale * crushHitboxMult, 0, crushFilter, results: crushResults) >= 2)
        {
            countCrushing++;
            // Have a buffer to avoid false positives
            if (countCrushing > 5)
            {
                // The player is just dead
                playerManager.KillPlayer();
            }
        }
        else
        {
            // Reset counter when not crushed
            countCrushing = 0;
        }
    }
}
