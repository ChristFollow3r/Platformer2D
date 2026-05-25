using UnityEngine;

namespace Sounds.Ambience
{
    [CreateAssetMenu(fileName = "BiomeAudioPlaylist", menuName = "Scriptable Objects/BiomeAudioPlaylist")]
    public class BiomeAudioPlaylist : ScriptableObject
    {
        public AudioClip[] tracks;

        public AudioClip GetRandomTrack()
        {
            if (tracks == null || tracks.Length == 0) return null;

            int randomIndex = Random.Range(0, tracks.Length);
            return tracks[randomIndex];
        }
    }
}
