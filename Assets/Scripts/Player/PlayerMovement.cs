    using UnityEngine;

    namespace Player // Rider yellow underlying was telling me to use a namespace
    {
        public class PlayerMovement : MonoBehaviour
        {
            private Rigidbody2D rb;
            private Collider2D playerCollider;
            private Animator playerAnimator;
            
            bool canDoulbeJump = false;
            
            public InputSystem_Actions playerInput;

            [SerializeField] private float speed;
            [SerializeField] private float jumpForce;
            private void Awake()
            {
                rb = GetComponent<Rigidbody2D>();   
                playerCollider = GetComponent<Collider2D>();
                playerAnimator = GetComponent<Animator>();
                playerInput = new InputSystem_Actions();
                playerInput.Enable();
            }
            private void Update()
            {
                rb.gravityScale = rb.linearVelocityY < 0 ? 5f : 3f;
                Movement();
            }

            private void Movement()
            {
                var movement = playerInput.Player.Move.ReadValue<Vector2>();
                rb.linearVelocityX = movement.x * speed;
                
                Vector2 origin = new Vector3(transform.position.x, playerCollider.bounds.min.y); // Dude still gets stuck on the wall
                var hit = Physics2D.Raycast(origin, Vector2.down, 0.2f);
                
                bool isGrounded = hit.collider is not null;
                if (isGrounded) canDoulbeJump = true;

                if (playerInput.Player.Jump.WasPerformedThisFrame())
                {
                    if (isGrounded)
                        rb.linearVelocityY = jumpForce;
                    
                    else if (canDoulbeJump)
                    {
                        rb.linearVelocityY = jumpForce;
                        canDoulbeJump = false;
                    }
                }
                
            }

            private void WallJump(bool isGrounded)
            {
                var leftHit = Physics2D.Raycast(playerCollider.bounds.min, Vector2.left, 0.2f);
                var rightHit = Physics2D.Raycast(playerCollider.bounds.min, Vector2.right, 0.2f);
            }
            
        }
    }

