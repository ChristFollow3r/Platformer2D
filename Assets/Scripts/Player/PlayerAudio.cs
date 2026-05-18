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

        [Header("Combat Audio Settings")]
        [SerializeField] private float pitchVariation = 0.1f;

        [Header("Movement Audio Settings")]
        [SerializeField] private float footstepMinPitch = 1.05f;
        [SerializeField] private float footstepMaxPitch = 1.3f;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
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

        private void PlayRandomized(AudioSource source, AudioClip clip, float minPitch, float maxPitch)
        {
            if (clip is null) return;
            source.pitch = Random.Range(minPitch, maxPitch);
            source.PlayOneShot(clip);
        }
    }
}
