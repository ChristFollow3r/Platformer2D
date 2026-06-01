
using System.Collections.Generic;
using System.Linq;
using Data;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Items.Utils
{

    public static class CookingUtils
    {
        private const string DatabasePath = "CookingRecipeDatabase";

        private static List<CookingRecipe> _recipes;
        public static IReadOnlyList<CookingRecipe> All
        {
            get
            {
                if (_recipes == null) Load();
                return _recipes;
            }
        }

        public static CookingRecipe Find(ItemData result) =>
            All.FirstOrDefault(r => r.result == result);

        public static IEnumerable<CookingRecipe> FindByIngredient(ItemData ingredient) =>
            All.Where(r => r.ingredients.Contains(ingredient));


        [RuntimeInitializeOnLoadMethod]
        static void Load()
        {
            if (_recipes != null) return;
            var db = Resources.Load<CookingRecipeDatabase>(DatabasePath);

            if (db == null)
            {
                Debug.LogError($"[RecipeManager] RecipeDatabase not found in Resources/{DatabasePath}. " +
                               "Create the asset and place it there.");
                _recipes = new List<CookingRecipe>();
                return;
            }

            _recipes = new List<CookingRecipe>(db.recipes);
            Debug.Log($"[RecipeManager] Loaded {_recipes.Count} recipes.");
        }


        public static ItemStack EvaluateCook(List<ItemStack> items, out CookingRecipe cookingRecipe)
        {
            #region EvaluateCook
            int gridSize = 2;
            cookingRecipe = null;
            if (items == null) return null;

            foreach (CookingRecipe CookingRecipe in All)
            {
                if (CookingRecipe == null) continue;
                if (CookingRecipe.gridSize > gridSize) continue;

                int maxOffset = gridSize - CookingRecipe.gridSize;

                for (int rowOffset = 0; rowOffset <= maxOffset; rowOffset++)
                {
                    for (int colOffset = 0; colOffset <= maxOffset; colOffset++)
                    {
                        if (MatchesAtOffset(items, gridSize, CookingRecipe, rowOffset, colOffset))
                        {
                            if (CookingRecipe.result == null)
                            {
                                Debug.LogWarning($"[CraftingUtils] CookingRecipe found, but its 'result' is missing!");
                                return null;
                            }
                            cookingRecipe = CookingRecipe;
                            return new ItemStack(CookingRecipe.result) { amount = (short)CookingRecipe.amount };
                        }
                    }
                }
            }

            return null;
            #endregion
        }


        private static bool MatchesAtOffset(List<ItemStack> items, int gridSize, CookingRecipe cookingRecipe, int rowOffset, int colOffset)
        {
            // collect all non-null recipe slots and check they match
            for (int row = 0; row < cookingRecipe.gridSize; row++)
            {
                for (int col = 0; col < cookingRecipe.gridSize; col++)
                {
                    ItemData expected = cookingRecipe.ingredients[row * cookingRecipe.gridSize + col];
                    if (expected == null) continue; // ignore empty recipe slots

                    int gridRow = row + rowOffset;
                    int gridCol = col + colOffset;
                    int craftIndex = gridRow * gridSize + gridCol;

                    ItemData provided = craftIndex < items.Count ? items[craftIndex]?.data : null;
                    if (provided != expected) return false;
                }
            }

            // also make sure slots NOT covered by the recipe are empty
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    int recipeRow = row - rowOffset;
                    int recipeCol = col - colOffset;

                    bool inBounds = recipeRow >= 0 && recipeRow < cookingRecipe.gridSize
                                 && recipeCol >= 0 && recipeCol < cookingRecipe.gridSize;

                    ItemData expected = inBounds
                        ? cookingRecipe.ingredients[recipeRow * cookingRecipe.gridSize + recipeCol]
                        : null;

                    if (expected != null) continue; // already checked above

                    int craftIndex = row * gridSize + col;
                    ItemData provided = craftIndex < items.Count ? items[craftIndex]?.data : null;
                    if (provided != null) return false; // slot should be empty but isn't
                }
            }

            return true;
        }
    }
}
