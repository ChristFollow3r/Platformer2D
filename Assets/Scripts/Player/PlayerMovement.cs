using System;
using System.Collections;
using Shared;
using UnityEngine;
using UnityEngine.InputSystem;
using World;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {

        #region Singleton setup

        public static PlayerMovement Singleton;

        private void SetupSingleton()
        {
            #region SetupSingleton

            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
                return;
            }

            Singleton = this;

            #endregion
        }

        #endregion

        private static readonly int HasMined = Animator.StringToHash("hasMined");
        private static readonly int HasAttacked = Animator.StringToHash("hasAttacked");
        private static readonly int IsGrounded = Animator.StringToHash("isGrounded");
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int IsSliding = Animator.StringToHash("isSliding");
        private static readonly int HasBeenHit = Animator.StringToHash("hasBeenHit");
        private static readonly int YVelocity = Animator.StringToHash("yVelocity");

        private Rigidbody2D rb;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private Collider2D playerCollider;
        private Camera mainCamera;

        public InputSystem_Actions playerInput;

        [SerializeField] private LayerMask groundLayer;

        [Header("Shader Setup")]
        [SerializeField] private Material tilemapMaterial;

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

        [Header("Cooldown Settings")]
        [SerializeField] private float jumpCooldown = 0.1f;
        [SerializeField] private float attackCooldown = 0.2f;
        [SerializeField] private float mineCooldown = 0.2f;

        [Header("Death VFX")]
        [SerializeField] private ParticleSystem deathVFX;

        private bool canDoubleJump;
        private float wallJumpTime = 0.25f;

        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private float attackPauseTimer;

        private float jumpCooldownTimer;
        private float attackCooldownTimer;
        private float mineCooldownTimer;

        private Shared.Health playerHealth;
        private float knockbackTimer;

        private Vector2 lastAttackMousePosition;
        private Vector2 lastMineMousePosition;

        public bool isGrounded { get; private set; }

        public event Action<Vector2> OnMinePerformed;
        public event Action<Vector2> OnAttackPerformed;
        public event Action OnJumpPerformed;

        public float enableTimer = 1f;

        private void Awake()
        {
            SetupSingleton();
            rb = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerCollider = GetComponent<Collider2D>();
            playerHealth = GetComponent<Shared.Health>();
            playerInput = new InputSystem_Actions();
            playerInput.Enable();
        }

        void Start()
        {
            mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            playerHealth.OnKnockbackRecieved += SlimeHitAnimation;
            playerHealth.OnDeath += PLayerDeath;
        }

        private void OnDisable()
        {
            playerHealth.OnKnockbackRecieved -= SlimeHitAnimation;
            playerHealth.OnDeath -= PLayerDeath;
        }

        private void Update()
        {
            Shader.SetGlobalVector("_PlayerPosition", transform.position);

            if (enableTimer > 0)
            {
                enableTimer -= Time.deltaTime;
                if (enableTimer <= 0)
                {
                    rb.constraints &= ~RigidbodyConstraints2D.FreezePosition;
                }
            }

            if (transform.position.y < -10f)
            {
                WorldManager.Instance.RespawnPlayer(gameObject);
            }

            if (UIController.Singleton.isOverlayOpen || UIController.Singleton.isMenuOpen) return;

            Vector2 colSize = playerCollider.bounds.size;
            Vector2 colCenter = playerCollider.bounds.center;

            isGrounded = Physics2D.BoxCast(colCenter, new Vector2(colSize.x * 0.9f, colSize.y), 0f, Vector2.down, 0.1f, groundLayer);

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
        }

        private void UpdatePolishTimers(bool isGrounded)
        {
            if (isGrounded) coyoteTimeCounter = coyoteTime;
            else coyoteTimeCounter -= Time.deltaTime;

            if (playerInput.Player.Jump.WasPerformedThisFrame()) jumpBufferCounter = jumpBufferTime;
            else jumpBufferCounter -= Time.deltaTime;

            if (attackPauseTimer > 0f) attackPauseTimer -= Time.deltaTime;

            if (jumpCooldownTimer > 0f) jumpCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer > 0f) attackCooldownTimer -= Time.deltaTime;
            if (mineCooldownTimer > 0f) mineCooldownTimer -= Time.deltaTime;
            if (knockbackTimer > 0f) knockbackTimer -= Time.deltaTime;
        }

        private void Movement(bool isGrounded, bool isTouchingLeftWall, bool isTouchingRightWall)
        {
            if (knockbackTimer > 0f) return;

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

            if (jumpBufferCounter > 0f && jumpCooldownTimer <= 0f)
            {
                if (!isGrounded && isSliding)
                {
                    float direction = isTouchingLeftWall ? 1 : -1;
                    rb.linearVelocity = new Vector2(direction * wallJumpForce.x, wallJumpForce.y);
                    wallJumpTime = 0.25f;
                    canDoubleJump = true;

                    jumpBufferCounter = 0f;
                    jumpCooldownTimer = jumpCooldown;
                    OnJumpPerformed?.Invoke();
                }
                else if (coyoteTimeCounter > 0f)
                {
                    rb.linearVelocityY = jumpForce;

                    jumpBufferCounter = 0f;
                    coyoteTimeCounter = 0f;
                    jumpCooldownTimer = jumpCooldown;
                    OnJumpPerformed?.Invoke();
                }
                else if (canDoubleJump && !isSliding)
                {
                    rb.linearVelocityY = jumpForce;
                    canDoubleJump = false;

                    jumpBufferCounter = 0f;
                    jumpCooldownTimer = jumpCooldown;
                    OnJumpPerformed?.Invoke();
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
                spriteRenderer.flipX = moveInput > 0;
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
            if (!mainCamera) mainCamera = Camera.main;

            if (knockbackTimer > 0f) return;
            if (Player.UIController.Singleton.isMenuOpen || Player.UIController.Singleton.isOverlayOpen) return;

            bool isShiftPressed = Keyboard.current.shiftKey.isPressed;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (isShiftPressed)
                {
                    if (mineCooldownTimer <= 0f)
                    {
                        mineCooldownTimer = mineCooldown;
                        lastMineMousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                        spriteRenderer.flipX = lastMineMousePosition.x < transform.position.x;
                        attackPauseTimer = attackPauseDuration;
                        animator.SetTrigger(HasMined);
                    }
                }
                else
                {
                    if (attackCooldownTimer <= 0f)
                    {
                        attackCooldownTimer = attackCooldown;
                        lastAttackMousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                        spriteRenderer.flipX = lastAttackMousePosition.x < transform.position.x;
                        attackPauseTimer = attackPauseDuration;
                        animator.SetTrigger(HasAttacked);
                    }
                }
            }
        }

        public void TriggerAttackEvent()
        {
            OnAttackPerformed?.Invoke(lastAttackMousePosition);
        }

        public void TriggerMineEvent()
        {
            OnMinePerformed?.Invoke(lastMineMousePosition);
        }

        private void SlimeHitAnimation(int direction, float knockback)
        {
            knockbackTimer = 0.3f;
            animator.SetTrigger(HasBeenHit);

            spriteRenderer.flipX = direction == 1;

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(direction * knockback, 5f), ForceMode2D.Impulse);
            StartCoroutine(DamageJuice());
        }

        private System.Collections.IEnumerator DamageJuice()
        {
            Time.timeScale = 0f;
            spriteRenderer.color = Color.red;
            yield return new WaitForSecondsRealtime(0.1f);
            Time.timeScale = 1f;
            spriteRenderer.color = Color.white;
        }

        private void PLayerDeath()
        {
            if (deathVFX != null)
            {
                Instantiate(deathVFX, transform.position, Quaternion.identity);
            }

            spriteRenderer.enabled = false;
            playerCollider.enabled = false;
            rb.simulated = false;

            playerInput.Disable();

            StartCoroutine(DeathCoroutine());
        }

        private IEnumerator DeathCoroutine()
        {
            yield return new WaitForSeconds(3f);

            World.WorldManager.Instance.RespawnPlayer(this.gameObject);

            spriteRenderer.enabled = true;
            playerCollider.enabled = true;
            rb.simulated = true;

            playerInput.Enable();

            playerHealth.ResetHealth();
        }

        private Vector2 FindSafeSpawnPosition(Vector2 startPos)
        {
            Vector2 size = playerCollider.bounds.size * 0.8f;

            for (int i = 0; i < 20; i++)
            {
                Vector2 candidate = startPos + new Vector2(0, i * 0.2f);
                if (!Physics2D.OverlapBox(candidate, size, 0f, groundLayer))
                    return candidate;
            }

            return startPos;
        }

        public string Serialize()
        {
            int health = GetComponent<Health>().currentHealth;
            return JsonUtility.ToJson(new PlayerSaveData() { pos = transform.position, health = health, spawnPoint = WorldManager.Instance.currentSpawnPoint });
        }

        public void Deserialize(string json)
        {
            PlayerSaveData save = JsonUtility.FromJson<PlayerSaveData>(json);
            Vector2 safePos = FindSafeSpawnPosition(save.pos + new Vector2(0, 0.75f));

            rb.linearVelocity = Vector2.zero;
            rb.position = safePos;
            rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;
            Physics2D.SyncTransforms();
            WorldManager.Instance.currentSpawnPoint = save.spawnPoint;

            GetComponent<Health>().SetHealth(save.health);
        }
    }
}
