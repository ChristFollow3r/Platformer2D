    using UnityEngine;

    namespace Player // Rider yellow underlying was telling me to use a namespace
    {
        public class PlayerMovement : MonoBehaviour
        {
            private Rigidbody2D rb;
            private Collider2D playerCollider;
            private Animator playerAnimator;
            
            // private static readonly int IsMoving = Animator.StringToHash("isMoving");
            // private static readonly int IsMovingRight = Animator.StringToHash("isMovingRight");
            // I'm fucking up animations too much
            
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
                
                /*
                if (Mathf.Abs(rb.linearVelocityX) > 0)
                    playerAnimator.SetBool(IsMovingRight, rb.linearVelocityX > 0);
                else
                    playerAnimator.SetBool(IsMoving, false);
                 */
                
                Vector2 origin = new Vector3(transform.position.x, playerCollider.bounds.min.y); // Dude still gets stuck on the wall
                var hit = Physics2D.Raycast(origin, Vector2.down, 0.2f);
                if (playerInput.Player.Jump.WasPerformedThisFrame() && hit.collider is not null
                    && !hit.collider.CompareTag("Drop")) rb.linearVelocityY = jumpForce; 
            }
            
        }
    }

