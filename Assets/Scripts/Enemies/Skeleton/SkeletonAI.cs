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
        
        private Rigidbody2D        skeletonRigidBody;
        private CapsuleCollider2D  skeletonCollider;
        private Shared.Health      skeletonHealth;
        private Transform          target;
        private SkeletonAnimations animations;
        
        private static readonly int Hit = Animator.StringToHash("isHit");
        
        private int                direction;
        private bool               isGrounded;
        private bool               theresBlockInFront;
        private bool               isStunned;
        public event Action<bool, int> OnRange; 

        private void Awake()
        {
            skeletonRigidBody = GetComponent<Rigidbody2D>();
            skeletonCollider  = GetComponent<CapsuleCollider2D>();
            skeletonHealth    = GetComponent<Shared.Health>();
            animations        = GetComponent<SkeletonAnimations>();
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        private void Start() => isStunned = false;
        private void OnEnable() => skeletonHealth.OnKnockbackRecieved += HandleHit;
        private void OnDisable() => skeletonHealth.OnKnockbackRecieved -= HandleHit;

        private void Update()
        {
            if (target is null || isStunned) return;

            if (animations.isAttacking)
            {
                skeletonRigidBody.linearVelocityX = 0;
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);
            CheckForObstacles();

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
            direction = target.position.x > transform.position.x ? 1 : -1;
            skeletonRigidBody.linearVelocityX = direction * skeletonData.speed;
            transform.localScale = new Vector3(direction, 1, 1);
            skeletonRigidBody.gravityScale = skeletonRigidBody.linearVelocityY < 0 ? 5f : 3f;
        }

        private void CheckForObstacles()
        {
            isGrounded         = Physics2D.Raycast(skeletonCollider.bounds.min, Vector2.down, 0.1f, groundLayer).collider is not null;
            theresBlockInFront = Physics2D.Raycast(transform.position, Vector2.right * direction, 0.8f, groundLayer).collider is not null;
            
            if (isGrounded && theresBlockInFront)
            {
                skeletonRigidBody.linearVelocityY = skeletonData.jumpForce;
            }
        }

        private void HandleHit(int playerDirection, float knockback)
        {
            StartCoroutine(SkeletonHit(playerDirection, knockback));
        }

        private IEnumerator SkeletonHit(int playerDirection, float knockback)
        {
            isStunned                        = true;
            skeletonRigidBody.linearVelocity =  Vector2.zero;
            skeletonRigidBody.AddForce(new Vector2(playerDirection * knockback, 3f), ForceMode2D.Impulse);
            yield return new WaitForSeconds(0.3f);
            isStunned                        = false;

        }
    }
}