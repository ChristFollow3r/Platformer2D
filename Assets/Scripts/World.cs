using UnityEngine;

public class World 
{
    public int width = 200; // Idk why I have this here XD
    public int height = 100;

    public BlockType[] blocks;

    public World(int width, int height)
    {
        this.width = width;
        this.height = height;
        blocks = new BlockType[width * height]; // For my world dictionary
    }
    public BlockType GetBlockTypes(int x, int y) // Getter
    {
        return blocks[(y * width) + x]; 
    }

    public void SetBlockType(int x, int y, BlockType type) // Setter (Just for the block type).
    {
        blocks[(y * width) + x] = type;
    }

    public bool SafeCheck(int x, int y) // Will return true if it's safe to check; if it's out of bounds it won't crash.
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }
}
