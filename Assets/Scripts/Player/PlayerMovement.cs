using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        private static readonly int IsRunning = Animator.StringToHash("isRunning");
        private static readonly int IsJumping = Animator.StringToHash("hasJumped");
        private static readonly int IsFalling = Animator.StringToHash("isFalling");
        private static readonly int IsSliding = Animator.StringToHash("isSliding");
        private static readonly int HasLanded = Animator.StringToHash("hasLanded");
        private static readonly int HasMined = Animator.StringToHash("hasMined");
        private static readonly int HasAttacked = Animator.StringToHash("hasAttacked");
        private static readonly int IsIdling = Animator.StringToHash("isIdling");

        private Rigidbody2D rb;
        private Animator animator;
        private Collider2D playerCollider;
        private Camera mainCamera; // Cached to make Rider happy

        public InputSystem_Actions playerInput;

        [SerializeField] private LayerMask groundLayer;

        [Header("Movement settings")]
        [SerializeField] private float speed;
        [SerializeField] private float jumpForce;
        [SerializeField] private Vector2 wallJumpForce = new Vector2(7f, 12f);

        private bool canDoubleJump;
        private float wallJumpTime = 0.25f;

        public event Action<Vector2> OnMinePerformed;
        public event Action<Vector2> OnAttackPerformed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            playerCollider = GetComponent<Collider2D>();
            mainCamera = Camera.main; // Cache the camera once

            playerInput = new InputSystem_Actions();
            playerInput.Enable();
        }

        private void Update()
        {
            bool isGrounded = Physics2D.Raycast(playerCollider.bounds.center, Vector2.down, playerCollider.bounds.extents.y + 0.2f, groundLayer).collider is not null;

            bool isTouchingLeftWall = Physics2D.Raycast(playerCollider.bounds.center, Vector2.left, playerCollider.bounds.extents.x + 0.2f, groundLayer).collider is not null;

            bool isTouchingRightWall = Physics2D.Raycast(playerCollider.bounds.center, Vector2.right, playerCollider.bounds.extents.x + 0.2f, groundLayer).collider is not null;

            Movement(isGrounded, isTouchingLeftWall, isTouchingRightWall);
            HandleMouseInput();
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

            if (wallJumpTime > 0f)
                wallJumpTime -= Time.deltaTime;
            else
                rb.linearVelocityX = movement * speed;

            if (playerInput.Player.Jump.WasPerformedThisFrame())
            {
                if (!isGrounded && (isTouchingLeftWall || isTouchingRightWall))
                {
                    float direction = isTouchingLeftWall ? 1 : -1;
                    rb.linearVelocity = new Vector2(direction * wallJumpForce.x, wallJumpForce.y);

                    wallJumpTime = 0.25f;
                    canDoubleJump = true;
                }
                else if (isGrounded)
                {
                    rb.linearVelocityY = jumpForce;
                    animator.SetTrigger(IsJumping);
                }
                else if (canDoubleJump)
                {
                    rb.linearVelocityY = jumpForce;
                    canDoubleJump = false;
                    animator.SetTrigger(IsJumping);
                }
            }
        }

        private void PlayerAnimations(bool isGrounded, bool isTouchingLeftWall, bool isTouchingRightWall)
        {
            // Normal walking flip
            if (Mathf.Abs(rb.linearVelocityX) > 0.1f)
            {
                float direction = Mathf.Sign(rb.linearVelocityX);
                transform.localScale = new Vector3(direction, 1f, 1f);
            }

            if (!isGrounded)
            {
                animator.SetBool(IsIdling, false);
                animator.SetBool(IsRunning, false);

                bool isFalling = rb.linearVelocityY < -0.1 && !isTouchingLeftWall && !isTouchingRightWall;
                animator.SetBool(IsFalling, isFalling);
            }
            else
            {
                if (animator.GetBool(IsFalling))
                {
                    animator.SetBool(IsFalling, false);
                    animator.SetTrigger(HasLanded);
                }

                float moveInput = playerInput.Player.Move.ReadValue<Vector2>().x;
                bool isMoving = Mathf.Abs(moveInput) > 0.1f;

                animator.SetBool(IsRunning, isMoving);
                animator.SetBool(IsIdling, !isMoving);
            }
        }

        private void HandleMouseInput()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                float facingDirection = mousePosition.x < transform.position.x ? -1f : 1f;
                transform.localScale = new Vector3(facingDirection, 1f, 1f);

                animator.SetTrigger(HasAttacked);
                OnAttackPerformed?.Invoke(mousePosition);
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

                float facingDirection = mousePosition.x < transform.position.x ? -1f : 1f;
                transform.localScale = new Vector3(facingDirection, 1f, 1f);

                animator.SetTrigger(HasMined);
                OnMinePerformed?.Invoke(mousePosition);
            }
        }
    }
}
