using UnityEngine;
using Data;

namespace Scriptable_Objects_Scripts
{
    [CreateAssetMenu(fileName = "Prop", menuName = "Scriptable Objects/Prop")]
    public class Prop : ScriptableObject
    {
        public ScriptableObject drop;
        public Sprite sprite;
        public new string name;
        public PropType type;
        public int hardness;
    }
}
