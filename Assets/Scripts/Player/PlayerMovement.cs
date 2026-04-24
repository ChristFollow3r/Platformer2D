    using UnityEngine;

    namespace Player // Rider yellow underlying was telling me to use a namespace
    {
        public class PlayerMovement : MonoBehaviour
        {
            private static readonly int IsRunning = Animator.StringToHash("isRunning");
            private Rigidbody2D rb;
            private Animator animator;
            private Collider2D playerCollider;
            
            public InputSystem_Actions playerInput;

            [Header("Movement settings")]
            [SerializeField] private float speed;
            [SerializeField] private float jumpForce;
            [SerializeField] private Vector2 wallJumpForce = new Vector2(7f, 12f);

            private bool canDoubleJump;
            private float wallJumpTime = 0.25f;
            
            private void Awake()
            {
                rb = GetComponent<Rigidbody2D>();   
                animator = transform.GetChild(0).GetComponent<Animator>();
                playerCollider = GetComponent<Collider2D>();
                playerInput = new InputSystem_Actions();
                playerInput.Enable();
            }
            private void Update()
            {
                bool isGrounded = Physics2D.Raycast(playerCollider.bounds.center, Vector2.down, playerCollider.bounds.extents.y + 0.2f).collider is not null;
                bool isTouchingLeftWall =
                    Physics2D.Raycast(playerCollider.bounds.center, Vector2.left,
                        playerCollider.bounds.extents.x + 0.2f).collider is not null;
                bool isTouchingRightWall = Physics2D.Raycast(playerCollider.bounds.center, Vector2.right, playerCollider.bounds.extents.x + 0.2f).collider is not null;
                
                Movement(isGrounded, isTouchingLeftWall, isTouchingRightWall);
                PlayerAnimations(isGrounded);
            }

            private void Movement(bool isGrounded, bool isTouchingLeftWall, bool isTouchingRightWall)
            {
                float movement = playerInput.Player.Move.ReadValue<Vector2>().x;
                
                if (isGrounded) canDoubleJump = true;
                
                if (isTouchingLeftWall || isTouchingRightWall && !isGrounded)
                    rb.gravityScale = 1.5f;
                
                else rb.gravityScale = rb.linearVelocityY < 0 ? 5f : 3f;
                
                if (wallJumpTime > 0f)
                    wallJumpTime -= Time.deltaTime;
                
                else
                    rb.linearVelocityX = movement * speed;

                if (playerInput.Player.Jump.WasPerformedThisFrame())
                {
                    if (!isGrounded && (isTouchingLeftWall || isTouchingRightWall))
                    {
                        float direction = isTouchingLeftWall ?  1 : -1;
                        rb.linearVelocity = new Vector2(direction * wallJumpForce.x, wallJumpForce.y);
                        
                        wallJumpTime = 0.25f;
                        canDoubleJump = true;
                    }
                    
                    else if (isGrounded)
                        rb.linearVelocityY = jumpForce;
                    
                    else if (canDoubleJump)
                    {
                        rb.linearVelocityY = jumpForce;
                        canDoubleJump = false;
                    }
                }
                
            }

            private void PlayerAnimations(bool isGrounded)
            {
                if (rb.linearVelocityX != 0f && isGrounded)
                {
                    animator.SetBool(IsRunning, true);
                    if (rb.linearVelocityX > 0f)
                        transform.localScale = new Vector3(1f, 1f, 1f);
                    else if (rb.linearVelocityX < 0f)
                        transform.localScale = new Vector3(-1f, 1f, 1f);
                }
                
                else animator.SetBool(IsRunning, false);
            }
            
        }
    }

