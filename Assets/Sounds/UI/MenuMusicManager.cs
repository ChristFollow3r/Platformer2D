using System.Collections;
using UnityEngine;

namespace Sounds.UI
{
    [RequireComponent(typeof(AudioSource))]
    public class MenuMusicManager : MonoBehaviour
    {
        public AudioClip[] menuTracks;
        public float fadeDuration = 1.5f;

        private AudioSource audioSource;
        public static MenuMusicManager Instance;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            PlayRandomTrack();
        }

        void Update()
        {
            if (!audioSource.isPlaying && audioSource.volume > 0)
            {
                PlayRandomTrack();
            }
        }

        public void PlayRandomTrack()
        {
            if (menuTracks.Length == 0) return;
            int randomIndex = Random.Range(0, menuTracks.Length);
            audioSource.clip = menuTracks[randomIndex];
            audioSource.Play();
        }

        public void FadeOutAndDestroy()
        {
            StartCoroutine(FadeOutCoroutine());
        }

        private IEnumerator FadeOutCoroutine()
        {
            float startVolume = audioSource.volume;
            float timeElapsed = 0f;

            while (timeElapsed < fadeDuration)
            {
                timeElapsed += Time.deltaTime;
                audioSource.volume = Mathf.Lerp(startVolume, 0f, timeElapsed / fadeDuration);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
