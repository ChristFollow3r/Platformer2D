
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldManager : MonoBehaviour
{
    [SerializeField] private Block[] blocks;
    [SerializeField] private Tilemap tileMap;
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
        WorldData.world = new World(200, 100);
        GenerateWolrd();
        RenderWorld();
    }

    private void GenerateWolrd()
    {
        for (int i = 0; i < WorldData.world.width; i++)
        {
            for (int j = 0; j < WorldData.world.height; j++)
            {
                BlockType blockType = BlockType.Air;
                float noiseValue = Mathf.PerlinNoise(i * scale, 0 );
                int groundLevel = (int)(noiseValue * WorldData.world.width * 0.5);

                if (j > groundLevel) blockType = BlockType.Air;
                else if (i == groundLevel) blockType = BlockType.Grass;
                else if (j >= groundLevel - 4) blockType = BlockType.Dirt; // I asked AI how to put the Random.Range cause it was conflicting with another library
                else blockType = BlockType.Stone;

                WorldData.world.SetBlockType(i, j, blockType);
            }
        }
    }

    private void RenderWorld()
    {
        for (int i = 0; i < WorldData.world.width; i++)
        {
            for (int j = 0; j < WorldData.world.height; j++)
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
