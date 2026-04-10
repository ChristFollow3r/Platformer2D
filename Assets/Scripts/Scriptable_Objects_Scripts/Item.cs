using UnityEngine;

namespace Scriptable_Objects_Scripts
{
    [CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
    public class Item : ScriptableObject
    {
        public string itemName;
        public Sprite itemIcon;
        public int maxStack;
    }

}