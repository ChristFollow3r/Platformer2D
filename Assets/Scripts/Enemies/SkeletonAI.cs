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
        private int direction;
        [Header("Attack Settings")]
        [SerializeField] private int attackDamage;
        [SerializeField] private float attackKnockback;
        [SerializeField] private Vector2 hitBoxSize;
        [SerializeField] private LayerMask playerLayerMask;

        public static float GetKnockBack { get; private set; }

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
        public static event Action<int, int> OnPlayerHit;

        private void Awake()
        {
            skeletonRigidBody = GetComponent<Rigidbody2D>();
            skeletonCollider =  GetComponent<CapsuleCollider2D>();
            skeletonAnimator = GetComponent<Animator>();
            target = GameObject.FindGameObjectWithTag("Player").transform;
            GetKnockBack = attackKnockback;
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

            if (distanceToPlayer <= 1f && !isAttacking)
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
            direction = target.position.x > transform.position.x ? 1 : -1;
            
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
            skeletonAnimator.SetBool(IsWalking, isWalking);
        }

        private IEnumerator Attack()
        {
            isAttacking = true;
            skeletonRigidBody.linearVelocityX = 0f;
            
            skeletonAnimator.SetBool(IsWalking, false);
            skeletonAnimator.SetBool(IsAttacking, true);
            
            yield return new WaitForSeconds(0.5f);
            
            Vector2 finalAttackPosition = (Vector2)transform.position + new Vector2(attackPosition.x * transform.localScale.x, attackPosition.y);
            Collider2D hit = Physics2D.OverlapBox(finalAttackPosition, hitBoxSize, 0, playerLayerMask);
            if (hit is not null)
            {
                OnPlayerHit?.Invoke(attackDamage, direction);
            }

            yield return new WaitForSeconds(0.17f);
            skeletonAnimator.SetBool(IsAttacking, false);
            
            yield return new WaitForSeconds(attackCooldown);
            isAttacking = false;
        }
    }
}
