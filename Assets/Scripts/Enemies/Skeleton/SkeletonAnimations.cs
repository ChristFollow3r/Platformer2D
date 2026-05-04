using System.Collections;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Enemies.Skeleton
{
    public class SkeletonAnimations : MonoBehaviour
    {
        [SerializeField] private Enemy skeletonData;
        
        private Animator      skeletonAnimator;
        private Rigidbody2D   skeletonRigidbody;
        private SkeletonAI    aiScript;
        private Shared.Health skeletonHealth;
        
        private static readonly int Walking = Animator.StringToHash("isWalking");
        private static readonly int Attacking = Animator.StringToHash("isAttacking");
        private static readonly int Hit = Animator.StringToHash("isHit");
        private static readonly int Dead = Animator.StringToHash("isDead");

        [HideInInspector] public bool isAttacking;

        private void Awake()
        {
            skeletonRigidbody = GetComponent<Rigidbody2D>();
            skeletonAnimator  = GetComponent<Animator>();
            aiScript          = GetComponent<SkeletonAI>();
            skeletonHealth    = GetComponent<Shared.Health>();
        }

        private void OnEnable()
        {
            aiScript.OnRange += HandleAttackTrigger;
            if (skeletonHealth != null)
            {
                skeletonHealth.OnKnockbackRecieved += PlayHitAnimation;
                skeletonHealth.OnDeath += PlayDeathAnimation;
            }
        }

        private void OnDisable()
        {
            aiScript.OnRange -= HandleAttackTrigger;
            if (skeletonHealth != null)
            {
                skeletonHealth.OnKnockbackRecieved -= PlayHitAnimation;
                skeletonHealth.OnDeath -= PlayDeathAnimation;
            }
        }

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

        private void PlayHitAnimation(int direction, float knockback)
        {
            StartCoroutine(SkeletonHitAnimation());
        }

        private IEnumerator SkeletonHitAnimation()
        {
            skeletonAnimator.SetBool(Hit, true);
            skeletonAnimator.SetBool(Walking, false);
            skeletonAnimator.SetBool(Attacking, false);
            yield return new WaitForSeconds(0.5f);
            skeletonAnimator.SetBool(Hit, false);
        }
        
        private void PlayDeathAnimation()
        {
            StopAllCoroutines();
            aiScript.enabled = false;
            skeletonRigidbody.bodyType = RigidbodyType2D.Static;
            if (TryGetComponent(out Collider2D col)) col.enabled = false;
                
            StartCoroutine(SkeletonDeathAnimation());
        }

        private IEnumerator SkeletonDeathAnimation()
        {
            skeletonAnimator.SetBool(Walking, false);
            skeletonAnimator.SetBool(Attacking, false);
            skeletonAnimator.SetBool(Hit, false);
    
            skeletonAnimator.SetTrigger(Dead);
            yield return new WaitForEndOfFrame();
            
            float animationLength = skeletonAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength);
            
            yield return new WaitForSeconds(2.0f);
            skeletonHealth.SpawnDeathDrops();
            Destroy(gameObject);
        }
    }
}