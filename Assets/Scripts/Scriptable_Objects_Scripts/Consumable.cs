using Data;
using UnityEngine;

namespace Scriptable_Objects_Scripts
{
  [CreateAssetMenu(fileName = "Consumable", menuName = "Scriptable Objects/Consumable")]
  public class Consumable : ItemData
  {
    public int saturation;
    public int healAmount;
  }
}
