using Data;
using UnityEngine;

namespace Scriptable_Objects_Scripts
{
  [CreateAssetMenu(fileName = "Tool", menuName = "Scriptable Objects/Tool")]
  public class Tool : ItemData
  {
    public int durability; // This does nothing so far
  }
}
