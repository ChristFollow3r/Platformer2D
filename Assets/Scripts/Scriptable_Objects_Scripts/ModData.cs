using Items.Overlays;
using UnityEngine;

namespace Scriptable_Objects_Scripts
{
    [CreateAssetMenu(fileName = "ModData", menuName = "Scriptable Objects/ModData")]
    public class ModData : ScriptableObject
    {
        public Mod mod;
        public float minigPower;
        public float duration;
    }
}
