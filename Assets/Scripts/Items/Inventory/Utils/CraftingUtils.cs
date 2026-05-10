
using System.Collections.Generic;
using System.Linq;
using Data;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Items.Utils
{

  public static class CraftingUtils
  {
    private const string DatabasePath = "RecipeDatabase";

    private static List<Recipe> _recipes;
    public static IReadOnlyList<Recipe> All
    {
      get
      {
        if (_recipes == null) Load();
        return _recipes;
      }
    }

    public static Recipe Find(ItemData result) =>
        All.FirstOrDefault(r => r.result == result);

    public static IEnumerable<Recipe> FindByIngredient(ItemData ingredient) =>
        All.Where(r => r.ingredients.Contains(ingredient));


    [RuntimeInitializeOnLoadMethod]
    static void Load()
    {
      if (_recipes != null) return;
      var db = Resources.Load<RecipeDatabase>(DatabasePath);

      if (db == null)
      {
        Debug.LogError($"[RecipeManager] RecipeDatabase not found in Resources/{DatabasePath}. " +
                       "Create the asset and place it there.");
        _recipes = new List<Recipe>();
        return;
      }

      _recipes = new List<Recipe>(db.recipes);
      Debug.Log($"[RecipeManager] Loaded {_recipes.Count} recipes.");
    }


    public static ItemStack EvaluateCraft(List<ItemStack> items, int gridSize)
    {
      #region EvaluateCraft
      Debug.Log("Evaluating craft!");

      foreach (Recipe recipe in All)
      {
        if (recipe.gridSize > gridSize) continue;
        int maxOffset = gridSize - recipe.gridSize;

        for (int rowOffset = 0; rowOffset <= maxOffset; rowOffset++)
          for (int colOffset = 0; colOffset <= maxOffset; colOffset++)
            if (MatchesAtOffset(items, gridSize, recipe, rowOffset, colOffset))
              return new ItemStack() { data = recipe.result, amount = (short)recipe.amount };
      }

      return null;
      #endregion
    }


    private static bool MatchesAtOffset(List<ItemStack> items, int gridSize, Recipe recipe, int rowOffset, int colOffset)
    {
      for (int row = 0; row < gridSize; row++)
      {
        for (int col = 0; col < gridSize; col++)
        {
          int craftIndex = row * gridSize + col;
          ItemData provided = craftIndex < items.Count ? items[craftIndex]?.data : null;

          int recipeRow = row - rowOffset;
          int recipeCol = col - colOffset;

          bool inBounds = recipeRow >= 0 && recipeRow < recipe.gridSize
                       && recipeCol >= 0 && recipeCol < recipe.gridSize;

          if (inBounds)
          {
            ItemData expected = recipe.ingredients[recipeRow * 4 + recipeCol];
            if (expected != provided) return false;
          }
          else
          {
            // Slots outside the recipe area must be empty
            if (provided != null) return false;
          }
        }
      }
      return true;
    }
  }
}
