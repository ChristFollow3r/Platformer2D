using System;
using Chunks;
using Data;
using Scriptable_Objects_Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private Block[] blocks;
    [SerializeField] private Grid gridParent;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private string worldSeed;
    

    public int worldWidth = 150; // Using World width and height gives error :V
    public int worldHeight = 90;
    public static WorldManager Instance { get; private set; }
    
    private float seedOffset;
    public float tallMountains = 0.05f; // To be changed
    public float mediumMountains = 0.1f;
    public float smallMountains = 0.02f;

    public Chunk[,] chunks;
    private readonly int renderDistance = 1;
    private Vector3 cameraPosition;
    

    private void Awake()
    {
        foreach (var block in blocks) WorldData.BlockDictionary[block.type] = block;

        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        WorldData.World = new World(worldWidth, worldHeight);
        chunks = new Chunk[(worldWidth + 15) / 16, (worldHeight + 15) / 16]; // Plus 15 to round up
        cameraPosition = mainCamera.transform.position;
        seedOffset =  ComputeSeedOffset(worldSeed);
        GenerateWorld();
        PopulateChunks();
        UpdateChunks(); // Call it once at start cause in update we call it only when the camera moves
    }

    private void Update()
    {
        if (CheckCameraMovement())
            UpdateChunks(); // If the camera moves perform the logic to render stuff
    }

    private void GenerateWorld()
    {
        for (int x = 0; x < worldWidth; x++)
        {
                float noiseValue = 0f;
                noiseValue += Mathf.PerlinNoise((x * tallMountains) + seedOffset, 0) * 1.0f;
                noiseValue += Mathf.PerlinNoise((x * mediumMountains) + seedOffset, 0) * 0.3f;
                noiseValue += Mathf.PerlinNoise((x * smallMountains) + seedOffset, 0) * 0.1f;
                noiseValue /= 1.75f;
                var groundLevel = (int)(noiseValue * worldHeight * 0.75f);
            
            for (int y = 0; y < worldHeight; y++)
            {
                
                BlockType blockType;

                if (y > groundLevel) blockType = BlockType.Air;
                else if (y == groundLevel) blockType = BlockType.Grass;
                else if (y >= groundLevel - 5) blockType = BlockType.Dirt;
                else blockType = BlockType.Stone;

                // To add, wood (trees) , water, coal, copper, tin, iron, gold , silver

                WorldData.World.SetBlockType(x, y, blockType);
            }
        }
        
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                BlockType blockType = WorldData.World.GetBlockTypes(x, y);
                if (blockType == BlockType.Air) continue;

                float caveNoise = Mathf.PerlinNoise((x * 0.05f) + seedOffset, (y * 0.05f) + seedOffset);
                
                if (caveNoise < 0.35f) 
                    WorldData.World.SetBlockType(x, y, BlockType.Air);
            }
        }
    }

    private float ComputeSeedOffset(string seed) 
    { // AI helped me with this I wanted to make my perlin noise world generation less shit.
        if (string.IsNullOrEmpty(seed)) return 0f;
        uint hash = 2166136261; // This weird ass numbers are official constants (whatever that is)
        // from the FNV-1a hash algorithm

        foreach (char x in seed)
        {
            hash ^= (uint)x;
            hash *= 16777619; 
        }

        return (hash % 1000) / 10000f;
    }

    private void PopulateChunks()
    {
        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int y = 0; y < chunks.GetLength(1); y++)
            {
                var chunk = new GameObject();
                var tileMap =  chunk.AddComponent<Tilemap>();
                chunk.AddComponent<TilemapRenderer>();
                chunk.AddComponent<TilemapCollider2D>();
                chunk.name = $"Chunk_{x}_{y}";
                chunk.transform.parent = gridParent.transform;
                chunks[x, y] = new Chunk(false, new Vector2Int(x, y), tileMap);
            }
        }
    }

    private void UpdateChunks()
    {
        Vector2Int
            cameraChunk =
                new Vector2Int((int)cameraPosition.x / 16,
                    (int)cameraPosition.y /
                    16); // Since our chunks are a 16 x 16, if we divide by 16, we get the chunk we currently
        // are at.
        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int y = 0; y < chunks.GetLength(1); y++)
            {
                int xPosition = Mathf.Abs(x - cameraChunk.x);
                int yPosition = Mathf.Abs(y - cameraChunk.y);
                bool inRange =
                    xPosition <= renderDistance &&
                    yPosition <= renderDistance; // If true, load the chunk, if false, unload if it's loaded.

                if (inRange && !chunks[x, y].isLoaded) chunks[x, y].LoadChunk();
                else if (!inRange && chunks[x, y].isLoaded) chunks[x, y].UnLoadChunk();
            }
        }
    }

    private bool CheckCameraMovement()
    {
        if (mainCamera.transform.position != cameraPosition)
        {
            cameraPosition = mainCamera.transform.position;
            return true;
        }

        else return false;
    }

}