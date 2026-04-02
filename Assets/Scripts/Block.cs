using UnityEngine;

[CreateAssetMenu(fileName = "Block", menuName = "Scriptable Objects/Block")]
public class Block : ScriptableObject
{
    public string blockType;
    public Sprite sprite;
}
