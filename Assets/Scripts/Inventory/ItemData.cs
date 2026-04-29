using UnityEngine;


namespace Data
{
  [CreateAssetMenu(fileName = "Item", menuName = "Item")]
  public class ItemData : ScriptableObject
  {
    public Sprite sprite;
    public new string name;
    public Texture icon;
    public bool isStackable => stack < 0;
    public int stack = 64;
  }

}
