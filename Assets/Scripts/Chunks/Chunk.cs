using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Chunk
{
    public bool isLoaded;
    public Vector2Int chunkPosition;
    private Tilemap tileMap;
    public Chunk(bool isLoaded, Vector2Int chunkPosition, Tilemap tilemap)
    {
        this.isLoaded = isLoaded;
        this.chunkPosition = chunkPosition;
        this.tileMap = tilemap;
    }

    public void LoadChunk()
    {
        int x = chunkPosition.x * 16;
        int y = chunkPosition.y * 16;  

        for (int i = x; i < x + 16; i++)
        {
            for (int j = y; j < y + 16; j++)
            {
                if (i >= WorldData.world.width || j >= WorldData.world.height) continue;
                BlockType blockType = WorldData.world.GetBlockTypes(i, j);

                if (blockType == BlockType.Air) continue;

                Tile tile = ScriptableObject.CreateInstance<Tile>(); // So you cannot do new Tile() cause it gives error
                tile.sprite = WorldData.blockDictionary[blockType].sprite;
                tileMap.SetTile(new Vector3Int(i, j, 0), tile);
            }
        }

        isLoaded = true;
    }

    public void unLoadChunk()
    {
        int x = chunkPosition.x * 16;
        int y = chunkPosition.y * 16;

        for (int i = x; i < x + 16; i++)
        {
            for (int j = y; j < y + 16; j++)
            {
                tileMap.SetTile(new Vector3Int(i, j, 0), null); //  The sky will be a background Image, I won't have a separate tilemap for the sky.
            }
        }

        isLoaded = false;
    }
}
