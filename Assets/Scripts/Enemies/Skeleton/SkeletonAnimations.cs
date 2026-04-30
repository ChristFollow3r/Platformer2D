using System;
using System.Collections;
using Player;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Enemies.Skeleton
{
    public class SkeletonAnimations : MonoBehaviour
    {
        [SerializeField] private Enemy skeletonData;
        
        private Animator skeletonAnimator;
        private Rigidbody2D skeletonRigidbody;
        private SkeletonAI aiScript;
        
        private static readonly int Walking = Animator.StringToHash("isWalking");
        private static readonly int Attacking = Animator.StringToHash("isAttacking");
        private static readonly int Hit = Animator.StringToHash("isHit");
        private static readonly int Dead = Animator.StringToHash("isDead");
        
        [HideInInspector] public bool isAttacking = false;
        
        // Event now sends Damage, Direction, and Knockback Force
        public static event Action<int, int, float> OnPlayerHit;

        private void Awake()
        {
            skeletonRigidbody = GetComponent<Rigidbody2D>();
            skeletonAnimator = GetComponent<Animator>();
            aiScript = GetComponent<SkeletonAI>();
        }

        private void OnEnable()
        {
            aiScript.OnRange += HandleAttackTrigger;
        }
        private void OnDisable() => aiScript.OnRange -= HandleAttackTrigger;

        private void Update()
        {
            bool isWalking = Mathf.Abs(skeletonRigidbody.linearVelocityX) > 0.1f;
            skeletonAnimator.SetBool(Walking, isWalking && !isAttacking);
        }

        private void HandleAttackTrigger(bool isGrounded, int direction)
        {
            if (!isAttacking && isGrounded) StartCoroutine(Attack(direction));
        }

        private IEnumerator Attack(int direction)
        {
            isAttacking = true;
            skeletonRigidbody.linearVelocityX = 0f;
            
            skeletonAnimator.SetBool(Walking, false);
            skeletonAnimator.SetBool(Attacking, true);
            
            yield return new WaitForSeconds(0.5f);
            
            Vector2 finalAttackPosition = (Vector2)transform.position + new Vector2(skeletonData.attackOffset.x * transform.localScale.x, skeletonData.attackOffset.y);
            Collider2D hit = Physics2D.OverlapBox(finalAttackPosition, skeletonData.hitBoxSize, 0, skeletonData.playerLayer);
            
            if (hit is not null) // Fixed: changed from 'is null'
            {
                if (hit.TryGetComponent(out Shared.Health health))
                {
                    health.TakeDamage(skeletonData.attackDamage, direction, skeletonData.attackKnockback);
                }
            }

            yield return new WaitForSeconds(0.17f);
            skeletonAnimator.SetBool(Attacking, false);
            
            yield return new WaitForSeconds(skeletonData.attackCooldown);
            isAttacking = false;
        }
    }
}