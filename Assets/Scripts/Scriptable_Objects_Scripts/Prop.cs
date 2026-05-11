using UnityEngine;
using System.Collections.Generic;
using Data;

namespace Scriptable_Objects_Scripts
{
  [System.Serializable]
  public class Drop
  {
    public ItemData item;
    public int amount;
    [Range(0, 101)] public int dropChance = 100;
  }

  // TODO: WTF IS THIS
  [CreateAssetMenu(fileName = "Prop", menuName = "Scriptable Objects/Prop")]
  public class Prop : ScriptableObject
  {
    public List<Drop> drops = new List<Drop>();
    public Sprite sprite;
    public new string name;
    public PropType type;
    public int hardness;
    public int spawnRate;
  }
}
