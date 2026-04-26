using Data;
using UnityEngine;

namespace Scriptable_Objects_Scripts
{
    [CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
    public class Item : ScriptableObject
    {
        public GameObject drop;
        public BlockType blockType;
        public string itemName;
        public Sprite itemIcon;
        public int maxStack;
        public int tier; // Fuck this shit
    }

}