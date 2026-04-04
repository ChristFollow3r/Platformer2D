using System.Collections.Generic;

public enum BlockType { Air, Dirt, Grass, Stone, Sand }
public enum Hardness { Level1, Level2, Level3, Level4, Level5 }

public static class WorldData
{
    public static Dictionary<BlockType, Block> blockDictionary = new();
    public static World world;
}
