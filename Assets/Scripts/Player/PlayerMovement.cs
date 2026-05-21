using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        private static readonly int HasMined = Animator.StringToHash("hasMined");
        private static readonly int HasAttacked = Animator.StringToHash("hasAttacked");
        private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int IsSliding = Animator.StringToHash("isSliding");
        private static readonly int YVelocity = Animator.StringToHash("yVelocity");

        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Collider2D playerCollider;
        private Camera mainCamera;

        public InputSystem_Actions playerInput;

        [SerializeField] private LayerMask groundLayer;

        [Header("Movement settings")]
        [SerializeField] private float speed = 8f;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private Vector2 wallJumpForce = new Vector2(7f, 12f);

        [Header("Polish Settings")]
        [SerializeField] private float wallSlideSpeed = 2f;
        [SerializeField] private float fallVelocityThreshold = -5f;
        [SerializeField] private float coyoteTime = 0.15f;
        [SerializeField] private float jumpBufferTime = 0.15f;
        [SerializeField] private float attackPauseDuration = 0.25f;

        private bool canDoubleJump;
        private float wallJumpTime = 0.25f;

        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private float attackPauseTimer;
        private bool wasGrounded;
        private bool isJumpingPhase;

        public event Action<Vector2> OnMinePerformed;
        public event Action<Vector2> OnAttackPerformed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerCollider = GetComponent<Collider2D>();
            mainCamera = Camera.main;

            playerInput = new InputSystem_Actions();
            playerInput.Enable();
        }

        private void Update()
        {
            if (UIController.Singleton.isOverlayOpen) return;

            Vector2 colSize = playerCollider.bounds.size;
            Vector2 colCenter = playerCollider.bounds.center;

            bool isGrounded = Physics2D.BoxCast(colCenter, new Vector2(colSize.x * 0.9f, colSize.y), 0f, Vector2.down, 0.1f, groundLayer);

            float rayLength = (colSize.x / 2f) + 0.15f;
            Vector2 topRayPos = colCenter + new Vector2(0, colSize.y * 0.3f);
            Vector2 bottomRayPos = colCenter - new Vector2(0, colSize.y * 0.4f);
            Vector2 highRayPos = colCenter + new Vector2(0, colSize.y * 0.8f);

            bool leftWallTop = Physics2D.Raycast(topRayPos, Vector2.left, rayLength, groundLayer);
            bool leftWallBottom = Physics2D.Raycast(bottomRayPos, Vector2.left, rayLength, groundLayer);
            bool leftWallHigh = Physics2D.Raycast(highRayPos, Vector2.left, rayLength, groundLayer);
            bool isTouchingLeftWall = leftWallTop && leftWallBottom && leftWallHigh;

            bool rightWallTop = Physics2D.Raycast(topRayPos, Vector2.right, rayLength, groundLayer);
            bool rightWallBottom = Physics2D.Raycast(bottomRayPos, Vector2.right, rayLength, groundLayer);
            bool rightWallHigh = Physics2D.Raycast(highRayPos, Vector2.right, rayLength, groundLayer);
            bool isTouchingRightWall = rightWallTop && rightWallBottom && rightWallHigh;

            UpdatePolishTimers(isGrounded);
            HandleMouseInput();
            Movement(isGrounded, isTouchingLeftWall, isTouchingRightWall);
            PlayerAnimations(isGrounded, isTouchingLeftWall, isTouchingRightWall);

            wasGrounded = isGrounded;
        }

        private void UpdatePolishTimers(bool isGrounded)
        {
            if (isGrounded) coyoteTimeCounter = coyoteTime;
            else coyoteTimeCounter -= Time.deltaTime;

            if (playerInput.Player.Jump.WasPerformedThisFrame()) jumpBufferCounter = jumpBufferTime;
            else jumpBufferCounter -= Time.deltaTime;

            if (attackPauseTimer > 0f) attackPauseTimer -= Time.deltaTime;
        }

        private void Movement(bool isGrounded, bool isTouchingLeftWall, bool isTouchingRightWall)
        {
            float movement = playerInput.Player.Move.ReadValue<Vector2>().x;
            if (isGrounded) canDoubleJump = true;

            if (attackPauseTimer > 0f && !isGrounded)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
                return;
            }

            rb.gravityScale = rb.linearVelocityY < 0 ? 5f : 3f;

            bool isSliding = !isGrounded && rb.linearVelocityY < 0 &&
                             ((isTouchingLeftWall && movement < 0) || (isTouchingRightWall && movement > 0));

            if (isSliding)
                rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Max(rb.linearVelocityY, -wallSlideSpeed));

            if (wallJumpTime > 0f) wallJumpTime -= Time.deltaTime;
            else rb.linearVelocityX = movement * speed;

            if (jumpBufferCounter > 0f)
            {
                if (!isGrounded && isSliding)
                {
                    float direction = isTouchingLeftWall ? 1 : -1;
                    rb.linearVelocity = new Vector2(direction * wallJumpForce.x, wallJumpForce.y);
                    wallJumpTime = 0.25f;
                    canDoubleJump = true;
                    jumpBufferCounter = 0f;
                    isJumpingPhase = true;
                }
                else if (coyoteTimeCounter > 0f)
                {
                    rb.linearVelocityY = jumpForce;
                    jumpBufferCounter = 0f;
                    coyoteTimeCounter = 0f;
                    isJumpingPhase = true;
                }
                else if (canDoubleJump && !isSliding)
                {
                    rb.linearVelocityY = jumpForce;
                    canDoubleJump = false;
                    jumpBufferCounter = 0f;
                    isJumpingPhase = true;
                }
            }
        }

        private void PlayerAnimations(bool isGrounded, bool isTouchingLeftWall, bool isTouchingRightWall)
        {
            float moveInput = playerInput.Player.Move.ReadValue<Vector2>().x;
            bool isMoving = Mathf.Abs(moveInput) > 0.1f;
            bool isSliding = !isGrounded && rb.linearVelocityY < 0 &&
                             ((isTouchingLeftWall && moveInput < 0) || (isTouchingRightWall && moveInput > 0));

            if (isSliding)
            {
                spriteRenderer.flipX = isTouchingRightWall;
            }
            else if (Mathf.Abs(rb.linearVelocityX) > 0.1f && wallJumpTime <= 0)
            {
                spriteRenderer.flipX = rb.linearVelocityX < 0;
            }

            animator.SetBool(IsGrounded, isGrounded);
            animator.SetBool(IsMoving, isMoving);
            animator.SetBool(IsSliding, isSliding);
            animator.SetFloat(YVelocity, rb.linearVelocityY);
        }

        private void HandleMouseInput()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                spriteRenderer.flipX = mousePosition.x < transform.position.x;

                attackPauseTimer = attackPauseDuration;
                animator.SetTrigger(HasAttacked);
                OnAttackPerformed?.Invoke(mousePosition);
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                spriteRenderer.flipX = mousePosition.x < transform.position.x;

                attackPauseTimer = attackPauseDuration;
                animator.SetTrigger(HasMined);
                OnMinePerformed?.Invoke(mousePosition);
            }
        }
    }
}
