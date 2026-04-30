using System.Collections;
using UnityEngine;

namespace Player 
{
    public class PlayerMovement : MonoBehaviour
    {
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int IsJumping = Animator.StringToHash("isJumping");
        private static readonly int IsFalling = Animator.StringToHash("isFalling");
        private static readonly int IsSliding = Animator.StringToHash("isSliding");
        private static readonly int IsHit = Animator.StringToHash("isHit");
        
        private Rigidbody2D         rb;
        private Animator            animator;
        private Collider2D          playerCollider;
        private Shared.Health       playerHealth;
        public InputSystem_Actions  playerInput;

        [Header("Movement settings")]
        [SerializeField] private float   speed;
        [SerializeField] private float   jumpForce;
        [SerializeField] private Vector2 wallJumpForce = new Vector2(7f, 12f);

        private bool  canDoubleJump;
        private bool  isStunned;
        private float wallJumpTime = 0.25f;
        
        private void Awake()
        {
            rb             = GetComponent<Rigidbody2D>();   
            animator       = transform.GetChild(0).GetComponent<Animator>();
            playerCollider = GetComponent<Collider2D>();
            playerHealth   = GetComponent<Shared.Health>();
            playerInput    = new InputSystem_Actions();
            playerInput.Enable();
        }

        private void OnEnable()
        {
            if (playerInput != null) playerHealth.OnKnockbackRecieved += HandleHit;
        }

        private void OnDisable()
        {
            if (playerInput != null) playerHealth.OnKnockbackRecieved -= HandleHit;
        }

        private void Update()
        {
            if (isStunned) return;
            
            bool isGrounded = Physics2D.Raycast(playerCollider.bounds.center, Vector2.down, playerCollider.bounds.extents.y + 0.2f, 3).collider is not null;
            bool isTouchingLeftWall = Physics2D.Raycast(playerCollider.bounds.center, Vector2.left, playerCollider.bounds.extents.x + 0.2f, 3).collider is not null;
            bool isTouchingRightWall = Physics2D.Raycast(playerCollider.bounds.center, Vector2.right, playerCollider.bounds.extents.x + 0.2f, 3).collider is not null;
            
            Movement(isGrounded, isTouchingLeftWall, isTouchingRightWall);
            PlayerAnimations(isGrounded, isTouchingLeftWall, isTouchingRightWall);
        }

        private void Movement(bool isGrounded, bool isTouchingLeftWall, bool isTouchingRightWall)
        {
            float movement = playerInput.Player.Move.ReadValue<Vector2>().x;
            if (isGrounded) canDoubleJump = true;
            
            if ((isTouchingLeftWall || isTouchingRightWall) && !isGrounded)
                rb.gravityScale = 1.5f;
            else 
                rb.gravityScale = rb.linearVelocityY < 0 ? 5f : 3f;
            
            if (wallJumpTime > 0f) wallJumpTime -= Time.deltaTime;
            else rb.linearVelocityX = movement * speed;

            if (playerInput.Player.Jump.WasPerformedThisFrame())
            {
                if (!isGrounded && (isTouchingLeftWall || isTouchingRightWall))
                {
                    float direction = isTouchingLeftWall ? 1 : -1;
                    rb.linearVelocity = new Vector2(direction * wallJumpForce.x, wallJumpForce.y);
                    wallJumpTime = 0.25f;
                    canDoubleJump = true;
                }
                else if (isGrounded) rb.linearVelocityY = jumpForce;
                else if (canDoubleJump)
                {
                    rb.linearVelocityY = jumpForce;
                    canDoubleJump = false;
                }
            }
        }

        private void PlayerAnimations(bool isGrounded, bool isTouchingLeftWall, bool isTouchingRightWall)
        {
            if (rb.linearVelocityX > 0f) transform.localScale = new Vector3(1f, 1f, 1f);
            else if (rb.linearVelocityX < 0f) transform.localScale = new Vector3(-1f, 1f, 1f);
            
            bool isRunning = Mathf.Abs(rb.linearVelocityX) > 0f && isGrounded;
            animator.SetBool(IsRunning, isRunning);

            if (!isGrounded)
            {
                if (rb.linearVelocityY > 0.1f || playerInput.Player.Jump.WasPerformedThisFrame())
                {
                    animator.SetBool(IsJumping, true);
                    animator.SetBool(IsFalling, false);
                }
                else if (rb.linearVelocityY < -0.1f && !isTouchingLeftWall && !isTouchingRightWall)
                {
                    animator.SetBool(IsJumping, false);
                    animator.SetBool(IsFalling, true);
                }
                else if (rb.linearVelocityY < -0.1f && isTouchingLeftWall)
                {
                    transform.localScale = new Vector3(1f, 1f, 1f);
                    animator.SetBool(IsSliding, true);
                }
                else if (rb.linearVelocityY < -0.1f && isTouchingRightWall)
                {
                    transform.localScale = new Vector3(-1f, 1f, 1f);
                    animator.SetBool(IsSliding, true);
                }
            }
            else
            {
                animator.SetBool(IsJumping, false);
                animator.SetBool(IsFalling, false);
                animator.SetBool(IsSliding, false);
            }
        }

        // Receiving the knockback value from the SkeletonAnimations event
        private void HandleHit(int direction, float knockback)
        {
            StartCoroutine(PlayerHitAnimation(direction, knockback));
        }

        private IEnumerator PlayerHitAnimation(int direction, float knockback)
        {
            isStunned = true;
            
            rb.linearVelocity = Vector2.zero;
            rb.linearVelocity = new Vector2(knockback * direction, 4f);
            animator.SetBool(IsHit, true);
            
            yield return new WaitForSeconds(0.3f);
            
            animator.SetBool(IsHit, false);
            isStunned = false;
        }
    }
}