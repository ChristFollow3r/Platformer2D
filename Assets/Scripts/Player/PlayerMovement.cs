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
        private Camera mainCamera;

        public InputSystem_Actions playerInput;

        [SerializeField] private LayerMask groundLayer;

        [Header("Movement settings")] [SerializeField]
        private float speed = 8f;

        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private Vector2 wallJumpForce = new Vector2(7f, 12f);

        [Header("Polish Settings")] [Tooltip("Max downward speed when sliding down a wall")] [SerializeField]
        private float wallSlideSpeed = 2f;

        [Tooltip("Velocity required before the falling animation triggers")] [SerializeField]
        private float fallVelocityThreshold = -5f;

        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float jumpBufferTime = 0.15f;

        private bool canDoubleJump;
        private float wallJumpTime = 0.25f;

        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private bool wasGrounded;

        public event Action<Vector2> OnMinePerformed;
        public event Action<Vector2> OnAttackPerformed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            playerCollider = GetComponent<Collider2D>();
            mainCamera = Camera.main;

            playerInput = new InputSystem_Actions();
            playerInput.Enable();
        }

        private void Update()
        {
            Vector2 colSize = playerCollider.bounds.size;
            Vector2 colCenter = playerCollider.bounds.center;

            bool isGrounded = Physics2D.BoxCast(colCenter, new Vector2(colSize.x * 0.9f, colSize.y), 0f, Vector2.down,
                0.1f, groundLayer);

            float rayLength = (colSize.x / 2f) + 0.15f;
            Vector2 topRayPos = colCenter + new Vector2(0, colSize.y * 0.3f);
            Vector2 bottomRayPos = colCenter - new Vector2(0, colSize.y * 0.4f);

            bool leftWallTop = Physics2D.Raycast(topRayPos, Vector2.left, rayLength, groundLayer);
            bool leftWallBottom = Physics2D.Raycast(bottomRayPos, Vector2.left, rayLength, groundLayer);
            bool isTouchingLeftWall = leftWallTop && leftWallBottom;

            bool rightWallTop = Physics2D.Raycast(topRayPos, Vector2.right, rayLength, groundLayer);
            bool rightWallBottom = Physics2D.Raycast(bottomRayPos, Vector2.right, rayLength, groundLayer);
            bool isTouchingRightWall = rightWallTop && rightWallBottom;

            UpdatePolishTimers(isGrounded);
            Movement(isGrounded, isTouchingLeftWall, isTouchingRightWall);
            HandleMouseInput();
            PlayerAnimations(isGrounded, isTouchingLeftWall, isTouchingRightWall);

            wasGrounded = isGrounded;
        }

        private void UpdatePolishTimers(bool isGrounded)
        {
            if (isGrounded) coyoteTimeCounter = coyoteTime;
            else coyoteTimeCounter -= Time.deltaTime;

            if (playerInput.Player.Jump.WasPerformedThisFrame()) jumpBufferCounter = jumpBufferTime;
            else jumpBufferCounter -= Time.deltaTime;
        }

        private void Movement(bool isGrounded, bool isTouchingLeftWall, bool isTouchingRightWall)
        {
            float movement = playerInput.Player.Move.ReadValue<Vector2>().x;
            if (isGrounded) canDoubleJump = true;

            rb.gravityScale = rb.linearVelocityY < 0 ? 5f : 3f;

            bool isSliding = (isTouchingLeftWall || isTouchingRightWall) && !isGrounded && rb.linearVelocityY < 0;

            if (isSliding)
                rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Max(rb.linearVelocityY, -wallSlideSpeed));

            if (wallJumpTime > 0f) wallJumpTime -= Time.deltaTime;
            else rb.linearVelocityX = movement * speed;


            if (jumpBufferCounter > 0f)
            {
                if (!isGrounded && (isTouchingLeftWall || isTouchingRightWall))
                {
                    float direction = isTouchingLeftWall ? 1 : -1;
                    rb.linearVelocity = new Vector2(direction * wallJumpForce.x, wallJumpForce.y);

                    wallJumpTime = 0.25f;
                    canDoubleJump = true;
                    jumpBufferCounter = 0f;
                }
                else if (coyoteTimeCounter > 0f)
                {
                    rb.linearVelocityY = jumpForce;
                    animator.SetTrigger(IsJumping);
                    jumpBufferCounter = 0f;
                    coyoteTimeCounter = 0f;
                }
                else if (canDoubleJump && !isSliding)
                {
                    rb.linearVelocityY = jumpForce;
                    canDoubleJump = false;
                    animator.SetTrigger(IsJumping);
                    jumpBufferCounter = 0f;
                }
            }
        }

        private void PlayerAnimations(bool isGrounded, bool isTouchingLeftWall, bool isTouchingRightWall)
        {
            bool isSliding = (isTouchingLeftWall || isTouchingRightWall) && !isGrounded && rb.linearVelocityY < 0;
            animator.SetBool(IsSliding, isSliding);

            if (isSliding)
            {
                float direction = isTouchingLeftWall ? 1f : -1f;
                transform.localScale = new Vector3(direction, 1f, 1f);
            }
            else if (Mathf.Abs(rb.linearVelocityX) > 0.1f && wallJumpTime <= 0)
            {
                float direction = Mathf.Sign(rb.linearVelocityX);
                transform.localScale = new Vector3(direction, 1f, 1f);
            }

            float moveInput = playerInput.Player.Move.ReadValue<Vector2>().x;
            bool isMoving = Mathf.Abs(moveInput) > 0.1f;

            if (!isGrounded)
            {
                bool isActuallyFalling = rb.linearVelocityY < fallVelocityThreshold && !isSliding;
                bool isJumpingUp = rb.linearVelocityY > 0.1f;

                animator.SetBool(IsFalling, isActuallyFalling);

                if (isActuallyFalling || isJumpingUp || isSliding)
                {
                    animator.SetBool(IsIdling, false);
                    animator.SetBool(IsRunning, false);
                }
                else
                {
                    animator.SetBool(IsRunning, isMoving);
                    animator.SetBool(IsIdling, !isMoving);
                }
            }
            else
            {
                if (!wasGrounded)
                {
                    animator.SetBool(IsFalling, false);
                    animator.SetTrigger(HasLanded);
                }
                else animator.SetBool(IsFalling, false);

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
