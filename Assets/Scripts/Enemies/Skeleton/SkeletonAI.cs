using System;
using System.Collections;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Enemies.Skeleton
{
    public class SkeletonAI : MonoBehaviour
    {
        [SerializeField] private Enemy skeletonData;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private ParticleSystem dustParticles;

        private Rigidbody2D        skeletonRigidBody;
        private CapsuleCollider2D  skeletonCollider;
        private Shared.Health      skeletonHealth;
        private Transform          target;
        private SkeletonAnimations animations;
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
            skeletonRigidBody = GetComponent<Rigidbody2D>();
            skeletonCollider  = GetComponent<CapsuleCollider2D>();
            skeletonHealth    = GetComponent<Shared.Health>();
            animations        = GetComponent<SkeletonAnimations>();
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
            skeletonHealth.OnKnockbackRecieved += HandleHit;
            obstacleCheckCoroutine = StartCoroutine(ObstacleCheckRoutine());
        }

        private void OnDisable()
        {
            skeletonHealth.OnKnockbackRecieved -= HandleHit;
            if (obstacleCheckCoroutine != null) StopCoroutine(obstacleCheckCoroutine);
        }

        private void Update()
        {
            if (target is null || isStunned) return;

            if (animations.isAttacking)
            {
                skeletonRigidBody.linearVelocityX = 0;
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, target.position);

            if (distanceToPlayer <= skeletonData.attackRange)
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
            skeletonRigidBody.linearVelocityX = direction * skeletonData.speed;

            spriteRenderer.flipX = direction == -1;
            skeletonRigidBody.gravityScale = skeletonRigidBody.linearVelocityY < 0 ? 5f : 3f;
        }

        private IEnumerator ObstacleCheckRoutine()
        {
            while (true)
            {
                if (target is not null && !isStunned && !animations.isAttacking)
                {
                    isGrounded = Physics2D.Raycast(skeletonCollider.bounds.min, Vector2.down, 0.1f, groundLayer).collider is not null;
                    theresBlockInFront = Physics2D.Raycast(new Vector2(transform.position.x, skeletonCollider.bounds.min.y + 0.1f),
                                 Vector2.right * direction, 0.6f, groundLayer).collider is not null;

                    if (isGrounded)
                    {
                        isKnockedBack = false;
                    }

                    if (isGrounded && theresBlockInFront)
                    {
                        skeletonRigidBody.linearVelocityY = skeletonData.jumpForce;

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
            StartCoroutine(SkeletonHit(playerDirection, knockback));
        }

        private IEnumerator SkeletonHit(int playerDirection, float knockback)
        {
            isStunned = true;
            isKnockedBack = true;

            skeletonRigidBody.linearVelocity = Vector2.zero;
            skeletonRigidBody.AddForce(new Vector2(playerDirection * knockback, 4f), ForceMode2D.Impulse);

            yield return new WaitForSeconds(0.3f);
            isStunned = false;
        }
    }
}
