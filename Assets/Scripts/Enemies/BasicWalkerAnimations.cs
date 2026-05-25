using System.Collections;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Enemies
{
    public class BasicWalkerAnimations : MonoBehaviour
    {
        [Header("Enemy Data")]
        [SerializeField] private Enemy enemyData;

        [Header("Animation Timings")]
        [SerializeField] private float attackAnimationDuration = 0.67f;
        [SerializeField] private float hitAnimationDuration = 0.5f;
        [SerializeField] private float corpseDespawnTime = 2.0f;

        private Animator      animator;
        private Rigidbody2D   rb;
        private BasicWalkerAI aiScript;
        private Shared.Health health;

        private static readonly int WalkingBool = Animator.StringToHash("isWalking");
        private static readonly int AttackTrigger = Animator.StringToHash("hasAttacked");
        private static readonly int HitTrigger = Animator.StringToHash("hasBeenHit");
        private static readonly int DeadTrigger = Animator.StringToHash("isDead");

        [HideInInspector] public bool isAttacking;
        private int currentAttackDirection;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            animator  = GetComponent<Animator>();
            aiScript          = GetComponent<BasicWalkerAI>();
            health    = GetComponent<Shared.Health>();
        }

        private void OnEnable()
        {
            aiScript.OnRange += HandleAttackTrigger;
            if (health != null)
            {
                health.OnKnockbackRecieved += PlayHitAnimation;
                health.OnDeath += PlayDeathAnimation;
            }
        }

        private void OnDisable()
        {
            aiScript.OnRange -= HandleAttackTrigger;
            if (health != null)
            {
                health.OnKnockbackRecieved -= PlayHitAnimation;
                health.OnDeath -= PlayDeathAnimation;
            }
        }

        private void Update()
        {
            bool isWalking = Mathf.Abs(rb.linearVelocityX) > 0.1f;
            animator.SetBool(WalkingBool, isWalking && !isAttacking);
        }

        private void HandleAttackTrigger(bool isGrounded, int direction)
        {
            if (!isAttacking && isGrounded) StartCoroutine(AttackCoroutine(direction));
        }

        private IEnumerator AttackCoroutine(int direction)
        {
            isAttacking = true;
            currentAttackDirection = direction;
            rb.linearVelocityX = 0f;

            animator.SetBool(WalkingBool, false);
            animator.SetTrigger(AttackTrigger);

            yield return new WaitForSeconds(attackAnimationDuration);

            yield return new WaitForSeconds(enemyData.attackCooldown);
            isAttacking = false;
        }

        public void TriggerAttackHitbox()
        {
            Vector2 finalAttackPosition = (Vector2)transform.position + new Vector2(enemyData.attackOffset.x * currentAttackDirection, enemyData.attackOffset.y);
            Collider2D hit = Physics2D.OverlapBox(finalAttackPosition, enemyData.hitBoxSize, 0, enemyData.playerLayer);

            if (hit is not null)
            {
                if (hit.TryGetComponent(out Shared.Health targetHealth))
                {
                    targetHealth.TakeDamage(enemyData.attackDamage, currentAttackDirection, enemyData.attackKnockback);
                }
            }
        }

        private void PlayHitAnimation(int direction, float knockback)
        {
            StartCoroutine(HitAnimationCoroutine());
        }

        private IEnumerator HitAnimationCoroutine()
        {
            // If they get hit, we cancel any pending attack animations
            animator.ResetTrigger(AttackTrigger);

            animator.SetTrigger(HitTrigger);
            animator.SetBool(WalkingBool, false);

            yield return new WaitForSeconds(hitAnimationDuration);
        }

        private void PlayDeathAnimation()
        {
            StopAllCoroutines();
            aiScript.enabled = false;

            rb.bodyType = RigidbodyType2D.Static;
            if (TryGetComponent(out Collider2D col)) col.enabled = false;

            StartCoroutine(DeathAnimationCoroutine());
        }

        private IEnumerator DeathAnimationCoroutine()
        {
            animator.SetBool(WalkingBool, false);

            // Clear out any other pending triggers so they don't play a hit animation while dying
            animator.ResetTrigger(AttackTrigger);
            animator.ResetTrigger(HitTrigger);

            animator.SetTrigger(DeadTrigger);
            yield return new WaitForEndOfFrame();

            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength);

            yield return new WaitForSeconds(corpseDespawnTime);

            health.SpawnDeathDrops();

            if (enemyData.deathParticles is not null)
            {
                var deathVFX = Instantiate(enemyData.deathParticles, transform.position, Quaternion.identity);
                Destroy(deathVFX.gameObject, deathVFX.main.duration);
            }

            Destroy(gameObject);
        }
    }
}
