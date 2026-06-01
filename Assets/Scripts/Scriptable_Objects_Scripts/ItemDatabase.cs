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
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemData> items = new();

#if UNITY_EDITOR
        [ContextMenu("Collect All Items")]
        private void CollectAll()
        {
            items = AssetDatabase.FindAssets("t:ItemData")
                .Select(guid => AssetDatabase.LoadAssetAtPath<ItemData>(
                                AssetDatabase.GUIDToAssetPath(guid)))
                .Where(r => r != null)
                .ToList();

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            Debug.Log($"[RecipeDatabase] Collected {items.Count} items.");
        }
#endif
    }
}

