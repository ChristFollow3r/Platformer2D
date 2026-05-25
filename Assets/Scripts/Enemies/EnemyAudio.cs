using System.Collections; // Added for IEnumerator
using Scriptable_Objects_Scripts;
using Shared;
using UnityEngine;

namespace Enemies
{
    public class EnemyAudio : MonoBehaviour
    {
        [SerializeField] private Enemy enemyData;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource enemyAudioSource;

        [Header("Combat Audio Settings")]
        [SerializeField] private float pitchVariation = 0.1f;

        [Header("Movement Audio Settings")]
        [SerializeField] private float footstepMinPitch = 1.05f;
        [SerializeField] private float footstepMaxPitch = 1.3f;

        private Health enemyHealth;
        private Coroutine gruntCoroutine; // Reference to the active coroutine

        private void Awake()
        {
            // Now it ONLY depends on the generic Health script
            enemyHealth = GetComponent<Health>();
        }

        private void OnEnable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.OnKnockbackRecieved += HandleHitEvent; // Use wrapper
                enemyHealth.OnDeath += PlayDeathSound;
            }

            // Start the random grunt loop
            gruntCoroutine = StartCoroutine(RandomGruntRoutine());
        }

        private void OnDisable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.OnKnockbackRecieved -= HandleHitEvent;
                enemyHealth.OnDeath -= PlayDeathSound;
            }

            // Clean up the coroutine if the object is disabled
            if (gruntCoroutine != null)
            {
                StopCoroutine(gruntCoroutine);
                gruntCoroutine = null;
            }
        }

        private void HandleHitEvent(int direction, float knockback)
        {
            PlayHitSound();
        }

        private IEnumerator RandomGruntRoutine()
        {
            while (true)
            {
                float waitTime = Random.Range(10f, 20f);

                yield return new WaitForSeconds(waitTime);

                PlayGruntSound();
            }
        }

        public void PlayMoveSound()
        {
            PlayRandomized(enemyAudioSource, enemyData.moveSound, footstepMinPitch, footstepMaxPitch);
        }

        public void PlayAttackSound()
        {
            PlayRandomized(enemyAudioSource, enemyData.attackSound, 1f - pitchVariation, 1f + pitchVariation);
        }

        public void PlayGruntSound()
        {
            PlayRandomized(enemyAudioSource, enemyData.gruntSound, 1f - pitchVariation, 1f + pitchVariation);
        }

        public void PlayHitSound()
        {
            PlayRandomized(enemyAudioSource, enemyData.hitSound, 1f - pitchVariation, 1f + pitchVariation);
        }

        public void PlayJumpSound()
        {
            PlayRandomized(enemyAudioSource, enemyData.jumpSound, 1.0f, 1.2f);
        }

        public void PlayDeathSound()
        {
            if (gruntCoroutine != null)
            {
                StopCoroutine(gruntCoroutine);
                gruntCoroutine = null;
            }

            if (enemyData.deathSound != null)
            {
                AudioSource.PlayClipAtPoint(enemyData.deathSound, transform.position);
            }
        }

        private void PlayRandomized(AudioSource source, AudioClip clip, float minPitch, float maxPitch)
        {
            if (clip == null || source == null) return;
            source.pitch = Random.Range(minPitch, maxPitch);
            source.PlayOneShot(clip);
        }
    }
}
