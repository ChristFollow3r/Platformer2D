using System;
using System.Collections;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Enemies
{
    public class BasicWalkerAI : MonoBehaviour
    {
        [Header("Enemy Data")]
        [SerializeField] private Enemy enemyData;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private ParticleSystem dustParticles;

        [Header("Physics Tuning")]
        [SerializeField] private float stunDuration = 0.3f;
        [SerializeField] private float knockbackLift = 4f;
        [SerializeField] private float defaultGravity = 3f;
        [SerializeField] private float fallGravity = 5f;

        private Rigidbody2D        rb;
        private CapsuleCollider2D  col;
        private Shared.Health      health;
        private Transform          target;
        private BasicWalkerAnimations animations;
        private SpriteRenderer     spriteRenderer;

        private int                direction;
        private bool               isGrounded;
        private bool               theresBlockInFront;
        private bool               isStunned;
        private bool               isKnockedBack;

        private WaitForSeconds     obstacleCheckDelay = new WaitForSeconds(0.1f);
        private Coroutine          obstacleCheckCoroutine;

        public event Action<bool, int> OnRange;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col  = GetComponent<CapsuleCollider2D>();
            health    = GetComponent<Shared.Health>();
            animations        = GetComponent<BasicWalkerAnimations>();
            spriteRenderer    = GetComponent<SpriteRenderer>();
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        private void Start()
        {
            isStunned = false;
            isKnockedBack = false;
        }

        private void OnEnable()
        {
            health.OnKnockbackRecieved += HandleHit;
            obstacleCheckCoroutine = StartCoroutine(ObstacleCheckRoutine());
        }

        private void OnDisable()
        {
            health.OnKnockbackRecieved -= HandleHit;
            if (obstacleCheckCoroutine != null) StopCoroutine(obstacleCheckCoroutine);
        }

        private void Update()
        {
            if (target is null || isStunned) return;

            if (animations.isAttacking)
            {
                rb.linearVelocityX = 0;
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, target.position);

            if (distanceToPlayer <= enemyData.attackRange)
            {
                OnRange?.Invoke(isGrounded, direction);
            }
            else
            {
                HandleMovement();
            }
        }

        private void HandleMovement()
        {
            if (isKnockedBack) return;

            direction = target.position.x > transform.position.x ? 1 : -1;
            rb.linearVelocityX = direction * enemyData.speed;

            spriteRenderer.flipX = direction == -1;
            rb.gravityScale = rb.linearVelocityY < 0 ? fallGravity : defaultGravity;
        }

        private IEnumerator ObstacleCheckRoutine()
        {
            while (true)
            {
                if (target is not null && !isStunned && !animations.isAttacking)
                {
                    isGrounded = Physics2D.Raycast(col.bounds.min, Vector2.down, 0.1f, groundLayer).collider is not null;
                    theresBlockInFront = Physics2D.Raycast(new Vector2(transform.position.x, col.bounds.min.y + 0.1f),
                                 Vector2.right * direction, 0.6f, groundLayer).collider is not null;

                    if (isGrounded)
                    {
                        isKnockedBack = false;
                    }

                    if (isGrounded && theresBlockInFront)
                    {
                        rb.linearVelocityY = enemyData.jumpForce;

                        if (TryGetComponent(out EnemyAudio enemyAudio))
                        {
                            enemyAudio.PlayJumpSound();
                        }
                    }
                }
                yield return obstacleCheckDelay;
            }
        }

        private void HandleHit(int playerDirection, float knockback)
        {
            StartCoroutine(TakeHitCoroutine(playerDirection, knockback));
        }

        private IEnumerator TakeHitCoroutine(int playerDirection, float knockback)
        {
            isStunned = true;
            isKnockedBack = true;

            rb.linearVelocity = Vector2.zero;
            rb.AddForce(new Vector2(playerDirection * knockback, knockbackLift), ForceMode2D.Impulse);

            yield return new WaitForSeconds(stunDuration);
            isStunned = false;
        }
    }
}
