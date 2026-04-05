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
}
