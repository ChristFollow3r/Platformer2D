// ─── RecipeDatabase.cs ────────────────────────────────────────────────────────
// Place the asset at:  Assets/Resources/RecipeDatabase.asset
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
  [CreateAssetMenu(fileName = "RecipeDatabase", menuName = "Scriptable Objects/Recipe Database")]
  public class RecipeDatabase : ScriptableObject
  {
    public List<Recipe> recipes = new();

#if UNITY_EDITOR
    [ContextMenu("Collect All Recipes")]
    private void CollectAll()
    {
      recipes = AssetDatabase.FindAssets("t:Recipe")
          .Select(guid => AssetDatabase.LoadAssetAtPath<Recipe>(
                          AssetDatabase.GUIDToAssetPath(guid)))
          .Where(r => r != null)
          .ToList();

      EditorUtility.SetDirty(this);
      AssetDatabase.SaveAssets();
      Debug.Log($"[RecipeDatabase] Collected {recipes.Count} recipes.");
    }
#endif
  }
}

