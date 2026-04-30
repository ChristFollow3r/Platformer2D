using System;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Enemies.Skeleton
{
    public class SkeletonAI : MonoBehaviour
    {
        [SerializeField] private Enemy skeletonData;
        private Rigidbody2D        skeletonRigidBody;
        private CapsuleCollider2D  skeletonCollider;
        private Transform          target;
        private SkeletonAnimations animations;

        private int                direction;
        private bool               isGrounded;
        private bool               theresBlockInFront;
        
        public event Action<bool, int> OnRange; 

        private void Awake()
        {
            skeletonRigidBody = GetComponent<Rigidbody2D>();
            skeletonCollider  = GetComponent<CapsuleCollider2D>();
            animations        = GetComponent<SkeletonAnimations>();
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        private void Update()
        {
            if (target is null) return;

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
            isGrounded         = Physics2D.Raycast(skeletonCollider.bounds.min, Vector2.down, 0.1f).collider is not null;
            theresBlockInFront = Physics2D.Raycast(transform.position, Vector2.right * direction, 0.6f).collider is not null;
            
            if (isGrounded && theresBlockInFront)
            {
                skeletonRigidBody.linearVelocityY = skeletonData.jumpForce;
            }
        }
    }
}