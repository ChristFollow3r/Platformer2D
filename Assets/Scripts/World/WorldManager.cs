using System.Collections.Generic;
using Chunks;
using Data;
using Scriptable_Objects_Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace World
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] private ItemData[] blocks;
        [SerializeField] private Prop[] props;

        [SerializeField] private Grid gridParent;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private string worldSeed;

        [Header("Shader Setup")]
        [SerializeField]
        private Material tilemapMaterial;

        private Texture2D lightmapTexture;

        [Header("World Settings")] public int worldWidth = 150;
        public int worldHeight = 90;
        [SerializeField] private float globalSpawnChance;
        [SerializeField] private int dirtLayerThickness;

        public static WorldManager Instance { get; private set; }

        private float seedOffset;
        public float tallMountains = 0.05f;
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
            WorldData.World = new Data.World(worldWidth, worldHeight);
            chunks = new Chunk[(worldWidth + 15) / Chunk.ChunkSize, (worldHeight + 15) / Chunk.ChunkSize];
            cameraPosition = mainCamera.transform.position;
            seedOffset = ComputeSeedOffset(worldSeed);

            lightmapTexture = new Texture2D(worldWidth, worldHeight, TextureFormat.RGBAHalf, false);
            lightmapTexture.filterMode = FilterMode.Bilinear;
            lightmapTexture.wrapMode = TextureWrapMode.Clamp;

            tilemapMaterial.SetTexture("_LightMap", lightmapTexture);
            tilemapMaterial.SetVector("_WorldSize", new Vector2(worldWidth, worldHeight));
            tilemapMaterial.SetFloat("_CellSize", gridParent.cellSize.x);

            GenerateWorld();
            GenerateProps();
            CalculateLighting();
            ApplyLightingToTexture();
            PopulateChunks();
            UpdateChunks();
        }

        private void Update()
        {
            if (CheckCameraMovement())
                UpdateChunks();
        }

        private void GenerateWorld()
        {
            // Pass 1: Terrain and Ore Generation
            for (int x = 0; x < worldWidth; x++)
            {
                var noiseValue = 0f;
                noiseValue += Mathf.PerlinNoise((x * tallMountains) + seedOffset, 0) * 1.0f;
                noiseValue += Mathf.PerlinNoise((x * mediumMountains) + seedOffset, 0) * 0.15f;
                noiseValue += Mathf.PerlinNoise((x * smallMountains) + seedOffset, 0) * 0.05f;
                noiseValue /= 1.2f;
                noiseValue = Mathf.Lerp(0.4f, 0.6f, noiseValue);
                var groundLevel = (int)(noiseValue * worldHeight * 0.75f);

                for (int y = 0; y < worldHeight; y++)
                {
                    BlockType blockType;

                    if (y > groundLevel) blockType = BlockType.Air;
                    else if (y == groundLevel) blockType = BlockType.Grass;
                    else if (y >= groundLevel - dirtLayerThickness) blockType = BlockType.Dirt;
                    else blockType = GetOreOrStone(y);

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

                    if (caveNoise < 0.35f) WorldData.World.SetBlockType(x, y, BlockType.Air);
                }
            }
        }

        private BlockType GetOreOrStone(int y)
        {
            float roll = UnityEngine.Random.Range(0f, 100f);
            float currentProb = 0f;

            currentProb += 0.1f;
            if (roll < currentProb)
            {
                int gemRoll = UnityEngine.Random.Range(0, 5);
                return gemRoll switch
                {
                    0 => BlockType.Sapphire,
                    1 => BlockType.Emerald,
                    2 => BlockType.Topaz,
                    3 => BlockType.Onyx,
                    _ => BlockType.Ruby
                };
            }

            currentProb += 0.5f;
            if (roll < currentProb)
            {
                if (y < worldHeight * 0.3f) return BlockType.Iron;
            }

            currentProb += 0.5f;
            if (roll < currentProb) return BlockType.Tin;

            currentProb += 2.0f;
            if (roll < currentProb) return BlockType.Copper;

            currentProb += 2.5f;
            if (roll < currentProb) return BlockType.Coal;

            return BlockType.Stone;
        }

        private void GenerateProps()
        {
            GeneratePropTypePass(true);
            GeneratePropTypePass(false);
        }

        private void GeneratePropTypePass(bool isPriorityPass)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    if (!WorldData.World.SafeCheck(x, y) || !WorldData.World.SafeCheck(x, y + 1)) continue;

                    BlockType groundBlock = WorldData.World.GetBlockTypes(x, y);
                    if (WorldData.World.GetBlockTypes(x, y + 1) != BlockType.Air) continue;

                    bool isSurface = groundBlock == BlockType.Grass;
                    bool isUnderground = groundBlock == BlockType.Stone;
                    if (!isSurface && !isUnderground) continue;

                    List<Prop> validProps = new List<Prop>();
                    float totalWeight = 0;

                    foreach (var p in props)
                    {
                        if (p.isFromSurface != isSurface) continue;
                        if (p.hasPriority != isPriorityPass) continue;

                        if (HasRequiredSpace(x, y, p.requiredSpace))
                        {
                            validProps.Add(p);
                            totalWeight += p.spawnChance;
                        }
                    }

                    if (validProps.Count == 0) continue;
                    if (Random.Range(0f, 100f) > globalSpawnChance) continue;

                    float roll = Random.Range(0f, totalWeight);
                    float currentWeight = 0;

                    foreach (var prop in validProps)
                    {
                        currentWeight += prop.spawnChance;

                        if (roll <= currentWeight)
                        {
                            WorldData.World.SetPropType(x, y + 1, prop.type);
                            break;
                        }
                    }
                }
            }
        }

        private float ComputeSeedOffset(string seed)
        {
            if (string.IsNullOrEmpty(seed)) return 0f;
            uint hash = 2166136261;

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
                    var bRenderer = blockChunkChild.AddComponent<TilemapRenderer>();
                    bRenderer.material = tilemapMaterial;

                    blockChunkChild.AddComponent<TilemapCollider2D>();
                    blockChunkChild.AddComponent<CompositeCollider2D>();

                    blockChunkChild.GetComponent<TilemapCollider2D>().compositeOperation =
                        Collider2D.CompositeOperation.Merge;
                    blockChunkChild.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                    blockChunkChild.GetComponent<CompositeCollider2D>().geometryType =
                        CompositeCollider2D.GeometryType.Outlines;

                    var propChunkChild = new GameObject("props");
                    propChunkChild.transform.parent = chunk.transform;
                    propChunkChild.AddComponent<Tilemap>();
                    var pRenderer = propChunkChild.AddComponent<TilemapRenderer>();
                    pRenderer.material = tilemapMaterial;

                    var propCollider = propChunkChild.AddComponent<TilemapCollider2D>();
                    propCollider.isTrigger = true;

                    chunks[x, y] = new Chunk(false, new Vector2Int(x, y), blockChunkChild.GetComponent<Tilemap>(),
                        propChunkChild.GetComponent<Tilemap>());
                }
            }
        }

        private void UpdateChunks()
        {
            float cellSize = gridParent.cellSize.x;
            int cameraTileX = Mathf.FloorToInt(cameraPosition.x / cellSize);
            int cameraTileY = Mathf.FloorToInt(cameraPosition.y / cellSize);
            var cameraChunk = new Vector2Int(cameraTileX / Chunk.ChunkSize, cameraTileY / Chunk.ChunkSize);

            for (int x = 0; x < chunks.GetLength(0); x++)
            {
                for (int y = 0; y < chunks.GetLength(1); y++)
                {
                    int xPosition = Mathf.Abs(x - cameraChunk.x);
                    int yPosition = Mathf.Abs(y - cameraChunk.y);
                    bool inRange = xPosition <= renderDistance && yPosition <= renderDistance;

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

            return false;
        }


        private void CalculateLighting()
        {
            int width = WorldData.World.width;
            int height = WorldData.World.height;

            for (int i = 0; i < width; i++)
            {
                float currentSunlight = 1.0f;
                for (int j = height - 1; j >= 0; j--)
                {
                    BlockType blockType = WorldData.World.GetBlockTypes(i, j);
                    if (blockType != BlockType.Air) currentSunlight *= 0.82f;
                    WorldData.World.lightValues[i, j] = currentSunlight;
                }
            }

            for (int iteration = 0; iteration < 14; iteration++)
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        BlockType type = WorldData.World.GetBlockTypes(i, j);
                        float currentValue = WorldData.World.lightValues[i, j];
                        float neighbourMax = 0f;
                        if (i > 0) neighbourMax = Mathf.Max(neighbourMax, WorldData.World.lightValues[i - 1, j]);
                        if (i < width - 1)
                            neighbourMax = Mathf.Max(neighbourMax, WorldData.World.lightValues[i + 1, j]);
                        if (j > 0) neighbourMax = Mathf.Max(neighbourMax, WorldData.World.lightValues[i, j - 1]);
                        if (j < height - 1)
                            neighbourMax = Mathf.Max(neighbourMax, WorldData.World.lightValues[i, j + 1]);

                        float decay = (type == BlockType.Air) ? 0.94f : 0.84f;
                        float spreadValue = neighbourMax * decay;
                        if (spreadValue > currentValue) WorldData.World.lightValues[i, j] = spreadValue;
                    }
                }
            }
        }

        private void ApplyLightingToTexture()
        {
            float bloomMultiplier = 1.0f;
            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    float l = WorldData.World.lightValues[x, y] * bloomMultiplier;
                    lightmapTexture.SetPixel(x, y, new Color(l, l, l, 1f));
                }
            }

            lightmapTexture.Apply();
        }

        private bool HasRequiredSpace(int x, int y, int neededSpace)
        {
            if (neededSpace <= 0) return true;

            for (int i = -neededSpace; i <= neededSpace; i++)
            {
                int checkX = x + i;

                if (!WorldData.World.SafeCheck(checkX, y + 1)) return false;
                if (WorldData.World.GetPropType(checkX, y + 1) != PropType.None) return false;
                if (WorldData.World.GetBlockTypes(checkX, y + 1) != BlockType.Air) return false;

                if (Mathf.Abs(i) <= 1)
                {
                    if (!WorldData.World.SafeCheck(checkX, y)) return false;
                    if (WorldData.World.GetBlockTypes(checkX, y) == BlockType.Air) return false;
                }
            }

            return true;
        }
    }
}

