using System.Collections;
using UnityEngine;

namespace Player
{
    public class PlayerAudio : MonoBehaviour
    {
        private PlayerMovement playerMovement;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource movementAudioSource;
        [SerializeField] private AudioSource combatAudioSource;

        [Header("Sound Clips")]
        [SerializeField] private AudioClip footstepSound;
        [SerializeField] private AudioClip attackWhoosh;
        [SerializeField] private AudioClip tinyDropSound;
        [SerializeField] private AudioClip dropSound;
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip slideSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip gruntSound;

        [Header("Combat Audio Settings")]
        [SerializeField] private float pitchVariation = 0.1f;

        [Header("Movement Audio Settings")]
        [SerializeField] private float footstepMinPitch = 1.05f;
        [SerializeField] private float footstepMaxPitch = 1.3f;

        private Coroutine gruntCoroutine;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        private void OnEnable()
        {
            if (playerMovement != null)
            {
                playerMovement.OnJumpPerformed += PlayJumpSound;
            }

            gruntCoroutine = StartCoroutine(RandomGruntRoutine());
        }

        private void OnDisable()
        {
            if (playerMovement != null)
            {
                playerMovement.OnJumpPerformed -= PlayJumpSound;
            }

            if (gruntCoroutine != null)
            {
                StopCoroutine(gruntCoroutine);
                gruntCoroutine = null;
            }
        }

        private IEnumerator RandomGruntRoutine()
        {
            while (true)
            {
                float waitTime = Random.Range(30f, 120f);
                yield return new WaitForSeconds(waitTime);
                PlayGruntSound();
            }
        }

        public void PlayGruntSound()
        {
            PlayRandomized(combatAudioSource, gruntSound, 1f - pitchVariation, 1f + pitchVariation);
        }

        public void PlayFootstepSound()
        {
            if (playerMovement != null && playerMovement.isGrounded)
            {
                PlayRandomized(movementAudioSource, footstepSound, footstepMinPitch, footstepMaxPitch);
            }
        }

        public void PlayAttackWhoosh()
        {
            PlayRandomized(combatAudioSource, attackWhoosh, 1f - pitchVariation, 1f + pitchVariation);
        }

        public void PlayTinyDropSound()
        {
            PlayRandomized(movementAudioSource, tinyDropSound, footstepMinPitch, footstepMaxPitch);
        }

        public void PlayDropSound()
        {
            PlayRandomized(movementAudioSource, dropSound, 1f - pitchVariation, 1f + pitchVariation);
        }

        public void PlaySlideSound()
        {
            PlayRandomized(movementAudioSource, slideSound, 1f - pitchVariation, 1f + pitchVariation);
        }

        public void PlayJumpSound()
        {
            PlayRandomized(movementAudioSource, jumpSound, 1.0f, 1.2f);
        }

        public void PlayHitSound()
        {
            PlayRandomized(movementAudioSource, hitSound, 0.95f, 1.05f);
        }

        private void PlayRandomized(AudioSource source, AudioClip clip, float minPitch, float maxPitch)
        {
            if (clip is null) return;
            source.pitch = Random.Range(minPitch, maxPitch);
            source.PlayOneShot(clip);
        }
    }
}
