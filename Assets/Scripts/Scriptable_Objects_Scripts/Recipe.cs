using Data;
using UnityEngine;

namespace Scriptable_Objects_Scripts
{
  [CreateAssetMenu(fileName = "Recipe", menuName = "Scriptable Objects/Recipe")]
  public class Recipe : ScriptableObject
  {
    public ItemData result;
    public int amount;
    [Header("Recipe")]
    public ItemData[] ingredients = new ItemData[16];
  }
}
