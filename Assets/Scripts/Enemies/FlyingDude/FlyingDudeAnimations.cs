using System.Collections;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Enemies.FlyingDude
{
    public class FlyingDudeAnimations : MonoBehaviour
    {
        [SerializeField] private Enemy flyingDudeData;

        [Header("Sounds")]
        [SerializeField] private AudioClip dudeDeathSound;
        [SerializeField] private AudioClip hitSound;
        private AudioSource audioSource;
        private readonly float pitchVariation = 0.1f;

        private Animator      dudeAnimator;
        private Rigidbody2D   dudeRigidbody;
        private FlyingDudeAI  aiScript;
        private Shared.Health dudeHealth;

        private static readonly int Attacking = Animator.StringToHash("hasAttacked");
        private static readonly int Hit = Animator.StringToHash("hasBeenHit");
        private static readonly int Dead = Animator.StringToHash("hasDied");

        [HideInInspector] public bool isAttacking;

        private void Awake()
        {
            dudeRigidbody = GetComponent<Rigidbody2D>();
            dudeAnimator  = GetComponent<Animator>();
            aiScript      = GetComponent<FlyingDudeAI>();
            dudeHealth    = GetComponent<Shared.Health>();
            audioSource   = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            aiScript.OnRange += HandleAttackTrigger;
            if (dudeHealth != null)
            {
                dudeHealth.OnKnockbackRecieved += PlayHitAnimation;
                dudeHealth.OnDeath += PlayDeathAnimation;
            }
        }

        private void OnDisable()
        {
            aiScript.OnRange -= HandleAttackTrigger;
            if (dudeHealth != null)
            {
                dudeHealth.OnKnockbackRecieved -= PlayHitAnimation;
                dudeHealth.OnDeath -= PlayDeathAnimation;
            }
        }

        private void HandleAttackTrigger(int direction)
        {
            if (!isAttacking) StartCoroutine(Attack(direction));
        }

        private IEnumerator Attack(int direction)
        {
            isAttacking = true;
            dudeRigidbody.linearVelocity = Vector2.zero;

            dudeAnimator.SetTrigger(Attacking);

            yield return new WaitForSeconds(1.2f);

            Vector2 finalAttackPosition = (Vector2)transform.position + new Vector2(flyingDudeData.attackOffset.x * direction, flyingDudeData.attackOffset.y);
            Collider2D hitTarget = Physics2D.OverlapBox(finalAttackPosition, flyingDudeData.hitBoxSize, 0, flyingDudeData.playerLayer);

            if (hitTarget is not null)
            {
                PlayRandomizedSound(hitSound);
                if (hitTarget.TryGetComponent(out Shared.Health health))
                {
                    health.TakeDamage(flyingDudeData.attackDamage, direction, flyingDudeData.attackKnockback);
                }
            }
            yield return new WaitForSeconds(flyingDudeData.attackCooldown);
            isAttacking = false;
        }

        private void PlayHitAnimation(int direction, float knockback)
        {
            StartCoroutine(DudeHitAnimation());
        }

        private IEnumerator DudeHitAnimation()
        {
            dudeAnimator.SetTrigger(Hit);
            yield return new WaitForSeconds(0.6f);
        }

        private void PlayDeathAnimation()
        {
            StopAllCoroutines();
            aiScript.enabled = false;

            if (dudeDeathSound is not null) AudioSource.PlayClipAtPoint(dudeDeathSound, transform.position);

            dudeRigidbody.bodyType = RigidbodyType2D.Static;
            if (TryGetComponent(out Collider2D col)) col.enabled = false;

            StartCoroutine(DudeDeathAnimation());
        }

        private IEnumerator DudeDeathAnimation()
        {
            dudeAnimator.SetTrigger(Dead);
            yield return new WaitForEndOfFrame();

            float animationLength = dudeAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength);

            yield return new WaitForSeconds(2.0f);
            dudeHealth.SpawnDeathDrops();
            Destroy(gameObject);
        }

        private void PlayRandomizedSound(AudioClip clip)
        {
            if (clip is null) return;
            float randomPitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);

            audioSource.pitch = randomPitch;
            audioSource.PlayOneShot(clip);
        }
    }
}
