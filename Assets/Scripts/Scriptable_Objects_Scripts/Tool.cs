using UnityEngine;

namespace Scriptable_Objects_Scripts
{
    [CreateAssetMenu(fileName = "Tool", menuName = "Scriptable Objects/Tool")]
    public class Tool : Item
    {
        public int tier;
        public int durability;
    }
}
