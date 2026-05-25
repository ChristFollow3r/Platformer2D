using UnityEngine;
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

        [Header("Volume Settings")]
        [Range(0f, 1f)] public float maxMusicVolume = 0.5f;
        [Range(0f, 1f)] public float maxAmbientVolume = 0.5f;

        [Header("Time of Day Curves")]
        public AnimationCurve dayVolumeCurve;
        public AnimationCurve nightVolumeCurve;

        private AudioClip lastSurfaceClip;
        private AudioClip lastCaveClip;

        private void Start()
        {
            if (!dayAmbientSource.isPlaying) dayAmbientSource.Play();
            if (!nightAmbientSource.isPlaying) nightAmbientSource.Play();

            if (caveAmbientSource != null && !caveAmbientSource.isPlaying) caveAmbientSource.Play();

            surfaceMusicSource.loop = false;
            caveMusicSource.loop = false;

            PlayNextTrack(surfaceMusicSource, surfacePlaylist);
            PlayNextTrack(caveMusicSource, cavePlaylist);
        }

        private void Update()
        {
            if (!surfaceMusicSource.isPlaying) PlayNextTrack(surfaceMusicSource, surfacePlaylist);
            if (!caveMusicSource.isPlaying) PlayNextTrack(caveMusicSource, cavePlaylist);

            float surfaceAlpha = 1f;

            if (parallaxManager is not null)
            {
                surfaceAlpha = parallaxManager.CurrentSurfaceAlpha;
                surfaceMusicSource.volume = surfaceAlpha * maxMusicVolume;
                caveMusicSource.volume = (1f - surfaceAlpha) * maxMusicVolume;
            }

            if (dayNightCycle is not null)
            {
                float time = dayNightCycle.CurrentTime;

                dayAmbientSource.volume = dayVolumeCurve.Evaluate(time) * maxAmbientVolume * surfaceAlpha;
                nightAmbientSource.volume = nightVolumeCurve.Evaluate(time) * maxAmbientVolume * surfaceAlpha;
            }

            if (caveAmbientSource is not null)
            {
                caveAmbientSource.volume = (1f - surfaceAlpha) * maxAmbientVolume;
            }
        }

        private void PlayNextTrack(AudioSource source, BiomeAudioPlaylist playlist)
        {
            if (playlist is null || playlist.tracks.Length == 0) return;

            AudioClip nextClip = playlist.GetRandomTrack();
            AudioClip lastClip = (source == surfaceMusicSource) ? lastSurfaceClip : lastCaveClip;

            if (playlist.tracks.Length > 1)
            {
                while (nextClip == lastClip)
                {
                    nextClip = playlist.GetRandomTrack();
                }
            }

            if (source == surfaceMusicSource) lastSurfaceClip = nextClip;
            else lastCaveClip = nextClip;

            source.clip = nextClip;
            source.Play();
        }
    }
}
