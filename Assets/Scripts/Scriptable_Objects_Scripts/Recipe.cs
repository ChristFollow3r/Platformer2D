using UnityEngine;

namespace Scriptable_Objects_Scripts
{
    [CreateAssetMenu(fileName = "Recipe", menuName = "Scriptable Objects/Recipe")]
    public class Recipe : ScriptableObject
    {
        public Item result;
        public int amount;
        [Header("Recipe")]
        public Item[] ingredients = new Item[16];
    }
}
