using Chunks;
using Data;
using Scriptable_Objects_Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private Block[] blocks;
    [SerializeField] private Prop[] props;
    
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
        foreach (var prop in props) WorldData.PropDictionary[prop.type] = prop;

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
        chunks = new Chunk[(worldWidth + 15) / Chunk.ChunkSize, (worldHeight + 15) / Chunk.ChunkSize]; // Plus 15 to round up
        cameraPosition = mainCamera.transform.position;
        seedOffset =  ComputeSeedOffset(worldSeed);
        GenerateWorld();
        GenerateProps();
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
                var noiseValue = 0f;
                noiseValue += Mathf.PerlinNoise((x * tallMountains) + seedOffset, 0) * 1.0f;
                noiseValue += Mathf.PerlinNoise((x * mediumMountains) + seedOffset, 0) * 0.3f;
                noiseValue += Mathf.PerlinNoise((x * smallMountains) + seedOffset, 0) * 0.1f;
                noiseValue /= 1.75f;
                var groundLevel = (int)(noiseValue * worldHeight * 0.75f);
            
            for (int y = 0; y < worldHeight; y++)
            {
                
                BlockType blockType; // I think this is useless
                

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
                var blockType = WorldData.World.GetBlockTypes(x, y);
                if (blockType == BlockType.Air) continue;

                float caveNoise = Mathf.PerlinNoise((x * 0.05f) + seedOffset, (y * 0.05f) + seedOffset);
                
                if (caveNoise < 0.35f) 
                    WorldData.World.SetBlockType(x, y, BlockType.Air);
            }
        }
    }

    private void GenerateProps()
    {
        for (int i = 0; i < worldWidth; i++)
        {
            for (int j = 0; j < worldHeight; j++)
            {
                if (!WorldData.World.SafeCheck(i, j) || !WorldData.World.SafeCheck(i, j + 1)) continue;
                
                if (WorldData.World.GetBlockTypes(i, j) == BlockType.Grass && WorldData.World.GetBlockTypes(i, j + 1) == BlockType.Air)
                {
                    int chance = Random.Range(0, 100);
                    WorldData.World.SetPropType(i, j, (chance >= 70 ? PropType.Bush : PropType.None));
                }
                
                else WorldData.World.SetPropType(i, j, PropType.None);
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
            hash ^= x;
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
                if (chunks[x, y] != null) continue;
                
                var chunk = new GameObject();
                chunk.name = $"Chunk_{x}_{y}";
                chunk.transform.parent = gridParent.transform;
                
                var blockChunkChild = new GameObject("blocks");
                blockChunkChild.transform.parent = chunk.transform;
                blockChunkChild.AddComponent<Tilemap>();
                blockChunkChild.AddComponent<TilemapRenderer>();
                blockChunkChild.AddComponent<TilemapCollider2D>();
                
                var propChunkChild = new GameObject("props");
                propChunkChild.transform.parent = chunk.transform;
                propChunkChild.AddComponent<Tilemap>();
                propChunkChild.AddComponent<TilemapRenderer>();
                
                chunks[x, y] = new Chunk(false, new Vector2Int(x, y), blockChunkChild.GetComponent<Tilemap>(), propChunkChild.GetComponent<Tilemap>());
            }
        }
    }

    private void UpdateChunks()
    {
        var cameraChunk = new Vector2Int((int)cameraPosition.x / Chunk.ChunkSize, (int)cameraPosition.y / Chunk.ChunkSize); 
        // Since our chunks are 16 x 16, if we divide by 16, we get the chunk we currently are at.
        
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

    private bool CheckCameraMovement() // We check every time the camera moves. A bit much inefficient, but it works just fine.
    {
        if (mainCamera.transform.position != cameraPosition)
        {
            cameraPosition = mainCamera.transform.position;
            return true;
        }

        else return false;
    }

}