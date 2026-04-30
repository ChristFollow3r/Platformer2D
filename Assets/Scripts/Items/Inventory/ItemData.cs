using UnityEngine;


namespace Data
{
  [CreateAssetMenu(fileName = "Item", menuName = "Item")]
  public class ItemData : ScriptableObject
  {
    public new string name;
    public Sprite sprite;
    public bool isStackable => stack < 0;
    public int stack = 64;
  }

}
