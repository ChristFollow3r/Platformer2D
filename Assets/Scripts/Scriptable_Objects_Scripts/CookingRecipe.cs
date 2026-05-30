using Data;
using UnityEngine;

namespace Scriptable_Objects_Scripts
{
    [CreateAssetMenu(fileName = "Cooking Recipe", menuName = "Scriptable Objects/Cooking Recipe")]
    public class CookingRecipe : ScriptableObject
    {
        public ItemData result;
        public int amount;
        public float cookTime;
        [Header("Recipe")]
        public int gridSize = 2;
        public ItemData[] ingredients = new ItemData[4];
    }
}
