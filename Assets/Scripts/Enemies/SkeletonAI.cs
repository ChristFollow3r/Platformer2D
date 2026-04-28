using System.Collections;
using System;
using UnityEngine;

namespace Enemies
{
    public class SkeletonAI : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float speed;
        [SerializeField] private float jumpForce;
        [Header("Attack Settings")]
        [SerializeField] private Vector2 hitBoxSize;
        [SerializeField] private LayerMask playerLayerMask;

        private Rigidbody2D skeletonRigidBody;
        private CapsuleCollider2D skeletonCollider;
        private Animator skeletonAnimator;
        private Transform target;
        
        private static readonly int IsWalking = Animator.StringToHash("isWalking");
        private static readonly int IsAttacking = Animator.StringToHash("isAttacking");
        private static readonly int IsHit = Animator.StringToHash("isHit");
        private static readonly int IsDead =  Animator.StringToHash("isDead");
        
        private bool skeletonIsGrounded;
        private bool theresBlockInFront;
        
        private bool isAttacking = false;
        [SerializeField] private Vector2 attackPosition;
        [SerializeField] private float attackCooldown;
        public static event Action onPlayerHit;

        private void Awake()
        {
            skeletonRigidBody = GetComponent<Rigidbody2D>();
            skeletonCollider =  GetComponent<CapsuleCollider2D>();
            skeletonAnimator = GetComponent<Animator>();
            target = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void Update()
        {
            if (target is null) return;

            if (isAttacking)
            { 
                skeletonRigidBody.linearVelocityX = 0;
                return;
            }
            
            float distanceToPlayer = Vector3.Distance(transform.position, target.position);

            if (distanceToPlayer <= 1f)
                StartCoroutine(Attack());
            
            else
            {
                CheckForObstacles();
                HandleMovement();
                HandleAnimations(skeletonIsGrounded);
            }
        }

        private void HandleMovement()
        {
            int direction = target.position.x > transform.position.x ? 1 : -1;
            skeletonRigidBody.linearVelocityX = direction * speed;
            transform.localScale = new Vector3(direction, 1, 1);
            skeletonRigidBody.gravityScale = skeletonRigidBody.linearVelocityY < 0 ? 5f : 3f;
        }

        private void CheckForObstacles()
        {
            int direction = target.position.x > transform.position.x ? 1 : -1;
            
            skeletonIsGrounded = Physics2D.Raycast(skeletonCollider.bounds.min, Vector2.down, 0.1f).collider is not null;
            theresBlockInFront = Physics2D.Raycast(transform.position, Vector2.right * direction, 0.6f).collider is not null;
            
            if (skeletonIsGrounded && theresBlockInFront)
            {
                skeletonRigidBody.linearVelocityY = jumpForce;
            }
        }

        private void HandleAnimations(bool isGrounded)
        {
            bool isWalking = Mathf.Abs(skeletonRigidBody.linearVelocityX) > 0.1f && isGrounded;
            
            if (isWalking)
                skeletonAnimator.SetBool(IsWalking, true);
            
            else
                skeletonAnimator.SetBool(IsWalking, false);
        }

        private IEnumerator Attack()
        {
            isAttacking = true;
            skeletonRigidBody.linearVelocityX = 0f;
            skeletonAnimator.SetBool(IsAttacking, true);
            yield return new WaitForSeconds(0.6f);
            skeletonAnimator.SetBool(IsAttacking, false);
            Collider2D hit = Physics2D.OverlapBox(attackPosition, hitBoxSize, 0, playerLayerMask);
            if (hit is not null)
            {
                onPlayerHit?.Invoke();
            }
            yield return new WaitForSeconds(attackCooldown);
            isAttacking = false;
        }
    }
}
