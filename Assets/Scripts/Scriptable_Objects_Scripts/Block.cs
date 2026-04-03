using UnityEngine;

[CreateAssetMenu(fileName = "Block", menuName = "Scriptable Objects/Block")]
public class Block : ScriptableObject
{
    public string blockName;
    public Sprite sprite;
    public BlockType type;
    public Hardness hardness;
}
