// ─── RecipeDatabase.cs ────────────────────────────────────────────────────────
// Place the asset at:  Assets/Resources/CookingRecipeDatabase.asset
// Run "Collect All Recipes" from the asset's context menu after adding new ones.

using System.Collections.Generic;
using System.Linq;
using Scriptable_Objects_Scripts;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Data
{
    [CreateAssetMenu(fileName = "CookingRecipeDatabase", menuName = "Scriptable Objects/Cooking Recipe Database")]
    public class CookingRecipeDatabase : ScriptableObject
    {
        public List<CookingRecipe> recipes = new();

#if UNITY_EDITOR
        [ContextMenu("Collect All Recipes")]
        private void CollectAll()
        {
            recipes = AssetDatabase.FindAssets("t:CookingRecipe")
                .Select(guid => AssetDatabase.LoadAssetAtPath<CookingRecipe>(
                                AssetDatabase.GUIDToAssetPath(guid)))
                .Where(r => r != null)
                .ToList();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log($"[RecipeDatabase] Collected {recipes.Count} cooking recipes.");
        }
#endif
    }
}

