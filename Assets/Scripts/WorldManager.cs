
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private Block[] blocks;
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private Camera mainCamera;

    public int worldWidth = 150; // Using World width and height gives errr :V
    public int worldHeight = 90;
    public static WorldManager wManagerSingleton { get; private set; }
    public float tallMountains = 0.05f; // This names are the worst names ever but it does the trick.
    public float mediumMountains = 0.1f;
    public float smallMountains = 0.02f;

    private Chunk[,] chunks;
    private int renderDistance = 1;
    private Vector3 cameraPosition;

    public Tilemap TileMapGetter => tileMap;

    private void Awake()
    {
        foreach (var block in blocks) WorldData.blockDictionary[block.type] = block;

        if (wManagerSingleton != null)
        {
            Destroy(gameObject);
            return;
        }
        
        wManagerSingleton = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        WorldData.world = new World(worldHeight, worldWidth);
        chunks = new Chunk[(worldWidth + 15) / 16, (worldHeight + 15) / 16]; // Plus 15 to round up
        cameraPosition = mainCamera.transform.position;
        GenerateWorld();
        PopulateChunks();
        UpdateChunks(); // Call it once at start cause in update we call it only when the camera moves
    }

    private void Update()
    {
        if (CheckCameraMovement()) UpdateChunks(); // If the camera moves perform the logic to render or unrender chunks
    }

    private void GenerateWorld()
    {
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                BlockType blockType = BlockType.Air;
                float noiseValue = 0f; // From here
                noiseValue += Mathf.PerlinNoise(x * tallMountains, 0) * 1.0f; // Copy paste from claude cause my way of using perlin noise was making VERY ugly terrain and so I asked it to help me.
                noiseValue += Mathf.PerlinNoise(x * mediumMountains, 0) * 0.3f; 
                noiseValue += Mathf.PerlinNoise(x * smallMountains, 0) * 0.1f; 
                noiseValue /= 1.75f; // To here
                int groundLevel = (int)(noiseValue * worldHeight * 0.3); 

                if (y > groundLevel) blockType = BlockType.Air;
                else if (y == groundLevel) blockType = BlockType.Grass;
                else if (y >= groundLevel - 8) blockType = BlockType.Dirt; 
                else blockType = BlockType.Stone;

                WorldData.world.SetBlockType(x, y, blockType);
            }
        }
    }

    private void PopulateChunks()
    {
        for(int x = 0;x < chunks.GetLength(0); x++)
        {
            for (int y = 0; y < chunks.GetLength(1); y++)
            {
                chunks[x, y] = new Chunk(false, new Vector2Int(x, y), TileMapGetter);
            }
        }
    }
    private void UpdateChunks() // ********************************************************************************************************************************************* 
    {
        Vector2Int cameraChunk = new Vector2Int((int)cameraPosition.x / 16, (int)cameraPosition.y / 16); // Since our chunks are a 16 x 16, if we divide by 16, we get the chunk we currently
        // are at.
        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int y = 0; y < chunks.GetLength(1); y++)
            {
                int xPosition = Mathf.Abs(x - cameraChunk.x);
                int yPosition = Mathf.Abs(y - cameraChunk.y);
                bool inRange = xPosition <= renderDistance && yPosition <= renderDistance; // If true, load the chunk, if false, unload if its loaded.

                if (inRange && !chunks[x, y].isLoaded) chunks[x, y].LoadChunk();
                else if (!inRange && chunks[x, y].isLoaded) chunks[x, y].unLoadChunk();
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
