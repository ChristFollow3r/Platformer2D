using System.Collections;
using UnityEngine;
using UnityEngine.Audio; // Required for Audio Mixers
using World;
using World.Background;

namespace Sounds.Ambience
{
    public class DynamicAudioManager : MonoBehaviour
    {
        [Header("Dependencies")]
        public DayNightCycle dayNightCycle;
        public AutoParallaxManager parallaxManager;

        [Header("Playlists (Scriptable Objects)")]
        public BiomeAudioPlaylist surfacePlaylist;
        public BiomeAudioPlaylist cavePlaylist;

        [Header("Audio Sources")]
        public AudioSource dayAmbientSource;
        public AudioSource nightAmbientSource;
        public AudioSource caveAmbientSource;
        public AudioSource surfaceMusicSource;
        public AudioSource caveMusicSource;

        [Header("Audio Mixer Snapshots")]
        public AudioMixerSnapshot surfaceActiveSnapshot;
        public AudioMixerSnapshot caveActiveSnapshot;

        [Header("Pacing & Fading")]
        [Range(0f, 1f)] public float maxAmbientVolume = 0.5f;
        public float minDelayBetweenSongs = 15f;
        public float maxDelayBetweenSongs = 45f;
        public float trackEndFadeDuration = 4f;
        public float biomeTransitionDuration = 2f;

        [Header("Time of Day Curves")]
        public AnimationCurve dayVolumeCurve;
        public AnimationCurve nightVolumeCurve;

        private AudioClip lastSurfaceClip;
        private AudioClip lastCaveClip;

        // State Tracking
        private bool isPlayerInCave = false;
        private Coroutine activeMusicRoutine;

        private void Start()
        {
            if (!dayAmbientSource.isPlaying) dayAmbientSource.Play();
            if (!nightAmbientSource.isPlaying) nightAmbientSource.Play();
            if (caveAmbientSource != null && !caveAmbientSource.isPlaying) caveAmbientSource.Play();

            surfaceMusicSource.loop = false;
            caveMusicSource.loop = false;

            isPlayerInCave = parallaxManager != null && parallaxManager.CurrentSurfaceAlpha < 0.5f;

            if (isPlayerInCave) caveActiveSnapshot.TransitionTo(0f);
            else surfaceActiveSnapshot.TransitionTo(0f);

            StartActiveBiomeRoutine();
        }

        private void Update()
        {
            float surfaceAlpha = 1f;

            if (parallaxManager != null)
            {
                surfaceAlpha = parallaxManager.CurrentSurfaceAlpha;
                Debug.Log($"Surface Alpha: {surfaceAlpha}");

                bool currentlyInCave = surfaceAlpha < 0.5f;
                if (currentlyInCave != isPlayerInCave)
                {
                    isPlayerInCave = currentlyInCave;
                    HandleBiomeChange();
                }
            }

            if (dayNightCycle != null)
            {
                float time = dayNightCycle.CurrentTime;
                dayAmbientSource.volume = dayVolumeCurve.Evaluate(time) * maxAmbientVolume * surfaceAlpha;
                nightAmbientSource.volume = nightVolumeCurve.Evaluate(time) * maxAmbientVolume * surfaceAlpha;
            }

            if (caveAmbientSource != null)
            {
                caveAmbientSource.volume = (1f - surfaceAlpha) * maxAmbientVolume;
            }
        }

        private void HandleBiomeChange()
        {
            if (activeMusicRoutine != null) StopCoroutine(activeMusicRoutine);

            if (isPlayerInCave)
            {
                caveActiveSnapshot.TransitionTo(biomeTransitionDuration);
            }
            else
            {
                surfaceActiveSnapshot.TransitionTo(biomeTransitionDuration);
            }

            StartActiveBiomeRoutine();
        }

        private void StartActiveBiomeRoutine()
        {
            if (isPlayerInCave)
            {
                activeMusicRoutine = StartCoroutine(MusicRoutine(caveMusicSource, cavePlaylist, false));
            }
            else
            {
                activeMusicRoutine = StartCoroutine(MusicRoutine(surfaceMusicSource, surfacePlaylist, true));
            }
        }

        private IEnumerator MusicRoutine(AudioSource source, BiomeAudioPlaylist playlist, bool isSurface)
        {
            yield return new WaitForSeconds(Random.Range(2f, 5f));

            while (true)
            {
                if (playlist == null || playlist.tracks.Length == 0)
                {
                    yield return new WaitForSeconds(5f);
                    continue;
                }

                AudioClip nextClip = GetRandomTrack(playlist, isSurface ? lastSurfaceClip : lastCaveClip);

                if (isSurface) lastSurfaceClip = nextClip;
                else lastCaveClip = nextClip;

                source.clip = nextClip;
                source.volume = 1f;
                source.Play();

                while (source.isPlaying && (source.clip.length - source.time) > trackEndFadeDuration)
                {
                    yield return null;
                }

                if (source.isPlaying)
                {
                    yield return StartCoroutine(FadeTrackEnd(source, trackEndFadeDuration));
                }

                float delay = Random.Range(minDelayBetweenSongs, maxDelayBetweenSongs);
                yield return new WaitForSeconds(delay);
            }
        }

        private IEnumerator FadeTrackEnd(AudioSource source, float duration)
        {
            if (!source.isPlaying) yield break;

            float startVolume = source.volume;
            float fadeTimer = 0f;

            while (fadeTimer < duration)
            {
                fadeTimer += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, fadeTimer / duration);
                yield return null;
            }

            source.volume = 0f;
            source.Stop();
        }

        private AudioClip GetRandomTrack(BiomeAudioPlaylist playlist, AudioClip lastClip)
        {
            if (playlist.tracks.Length <= 1) return playlist.tracks[0];

            AudioClip nextClip = playlist.tracks[Random.Range(0, playlist.tracks.Length)];
            int safetyCounter = 0;

            while (nextClip == lastClip && safetyCounter < 20)
            {
                nextClip = playlist.tracks[Random.Range(0, playlist.tracks.Length)];
                safetyCounter++;
            }

            return nextClip;
        }
    }
}
