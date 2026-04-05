
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private Block[] blocks;
    [SerializeField] private Tilemap tileMap;

    public int worldWidth = 200; // It's duplicated cause I don't feel like writing every time WorldData.World.Width
    public int worldHeight = 100;
    public static WorldManager wManagerSingleton { get; private set; }
    public float scale = 0.05f;

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
        GenerateWolrd();
        RenderWorld();
    }

    private void GenerateWolrd()
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

    private void RenderWorld()
    {
        for (int i = 0; i < worldWidth; i++)
        {
            for (int j = 0; j < worldHeight; j++)
            {
                BlockType blockType = WorldData.world.GetBlockTypes(i, j);
                if (blockType == BlockType.Air) continue;

                Tile tile = ScriptableObject.CreateInstance<Tile>(); // AI helped me with this
                tile.sprite = WorldData.blockDictionary[blockType].sprite;
                if (tile.sprite == null) Debug.Log("FUCK");

                tileMap.SetTile(new Vector3Int (i, j, 0), tile);
            }
        }
    }
}
