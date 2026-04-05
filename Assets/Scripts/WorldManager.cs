
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private Block[] blocks;
    [SerializeField] private Tilemap tileMap;

    public int worldWidth = WorldData.world.width;
    public int worldHeight = WorldData.world.height;
    public static WorldManager wManagerSingleton { get; private set; }
    public float scale = 0.05f;

    private Chunk[,] chunks;

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
        GenerateWorld();
        PopulateChunks();
    }

    private void GenerateWorld()
    {
        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                BlockType blockType = BlockType.Air;
                float noiseValue = Mathf.PerlinNoise(x * scale, 0 );
                int groundLevel = (int)(noiseValue * worldHeight * 0.5); // * 0.5 so the terrain has a reasonable height

                if (y > groundLevel) blockType = BlockType.Air;
                else if (y == groundLevel) blockType = BlockType.Grass;
                else if (y >= groundLevel - 4) blockType = BlockType.Dirt; 
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
    private void RenderChunks()
    {

    }
    
}
