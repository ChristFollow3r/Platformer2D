using System.Collections.Generic;
using Chunks;
using Data;
using Scriptable_Objects_Scripts;
using UnityEngine;
using UnityEngine.Tilemaps;
using Unity.Cinemachine;
using Player;

namespace World
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] private ItemData[] blocks;
        [SerializeField] private Prop[] props;

        [SerializeField] private Grid gridParent;
        [SerializeField] private Camera mainCamera;

        [Header("Player Settings")] [SerializeField]
        private GameObject playerPrefab;

        [SerializeField] private CinemachineCamera virtualCamera;
        public Vector3 currentSpawnPoint;

        [Header("Shader Setup")] [SerializeField]
        private Material tilemapMaterial;

        private Texture2D lightmapTexture;
        private Color[] lightmapColors;

        private bool[,] isAirCache;
        private float[,] lightMapCache;

        [Header("Lighting Settings")]
        [SerializeField] private float verticalSolidDecay = 0.8f;
        [SerializeField] private float spreadSolidDecay = 0.90f;
        [SerializeField] private float spreadAirDecay = 0.7f;
        [SerializeField] private int lightUpdateRadius = 20;
        [SerializeField] private int lightIterations = 3;

        [Header("World Settings")] public int worldWidth = 150;
        public int worldHeight = 90;
        [SerializeField] private float globalSpawnChance;
        [SerializeField] private int dirtLayerThickness;

        [Header("Autosave")] [SerializeField] private float autosaveInterval = 60f;
        private float autosaveTimer;

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
            if (Instance != null && Instance != this)
            {
                Destroy(Instance.gameObject);
            }

            Instance = this;

            foreach (var block in blocks)
            {
                if (WorldData.BlockDictionary.TryGetValue(block.blockType, out ItemData current))
                {
                    Debug.LogWarning($"Block {block.name} / {block.sprite} is trying to override {block.blockType} held by {current.name} / {current.sprite}");
                }

                WorldData.BlockDictionary[block.blockType] = block;
            }

            foreach (var prop in props) WorldData.PropDictionary[prop.type] = prop;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            WorldData.dirtyBlocks.Clear();
            WorldData.dirtyProps.Clear();

            WorldData.World = new Data.World(worldWidth, worldHeight);
            chunks = new Chunk[(worldWidth + 15) / Chunk.ChunkSize, (worldHeight + 15) / Chunk.ChunkSize];
            cameraPosition = mainCamera.transform.position;

            lightmapTexture = new Texture2D(worldWidth, worldHeight, TextureFormat.RGBAHalf, false);
            lightmapTexture.filterMode = FilterMode.Point;
            lightmapTexture.wrapMode = TextureWrapMode.Clamp;

            lightmapColors = new Color[worldWidth * worldHeight];
            isAirCache = new bool[worldWidth, worldHeight];
            lightMapCache = new float[worldWidth, worldHeight];

            tilemapMaterial.SetTexture("_LightMap", lightmapTexture);
            tilemapMaterial.SetVector("_WorldSize", new Vector2(worldWidth, worldHeight));
            tilemapMaterial.SetFloat("_CellSize", gridParent.cellSize.x);

            if (WorldSerializer.isNewWorld) NewWorld();
            else LoadWorld();

            CalculateLighting();
            ApplyLightingToTexture();
            PopulateChunks();
            UpdateChunks();
            SpawnPlayer();

            if (!WorldSerializer.isNewWorld)
                WorldSerializer.LoadPlayer();
        }

        private int GetDeterministicHash(string seed)
        {
            if (string.IsNullOrEmpty(seed)) return UnityEngine.Random.Range(-100000, 100000);
            unchecked
            {
                int hash = 23;
                foreach (char c in seed)
                {
                    hash = hash * 31 + c;
                }

                return hash;
            }
        }

        private float ComputeSeedOffset(int seedHash)
        {
            System.Random prng = new System.Random(seedHash);
            return prng.Next(-100000, 100000);
        }

        private void NewWorld()
        {
            int seedHash = GetDeterministicHash(WorldSerializer.Seed);
            seedOffset = ComputeSeedOffset(seedHash);
            UnityEngine.Random.InitState(seedHash);

            WorldData.isGenerating = true;
            GenerateWorld();
            GenerateProps();
            WorldData.isGenerating = false;
            WorldSerializer.Save();
        }

        private void LoadWorld()
        {
            WorldSaveData save = WorldSerializer.Load();
            if (save == null)
            {
                Debug.LogError("[WorldManager] Save data is null, falling back to new world.");
                NewWorld();
                return;
            }

            int seedHash = GetDeterministicHash(save.seed);
            UnityEngine.Random.InitState(seedHash);

            seedOffset = ComputeSeedOffset(seedHash);

            WorldData.isGenerating = true;
            GenerateWorld();
            GenerateProps();
            WorldData.isGenerating = false;

            WorldData.isGenerating = true;
            foreach (var b in save.blocks)
                WorldData.World.SetBlockType(b.x, b.y, b.type);
            foreach (var p in save.props)
                WorldData.World.SetPropType(p.x, p.y, p.type);
            WorldData.isGenerating = false;

            foreach (var b in save.blocks)
                WorldData.dirtyBlocks.Add(new Vector2Int(b.x, b.y));
            foreach (var p in save.props)
                WorldData.dirtyProps.Add(new Vector2Int(p.x, p.y));

            WorldSerializer.LoadOverlays();
        }

        private void Update()
        {
            if (CheckCameraMovement())
                UpdateChunks();

            if (!string.IsNullOrEmpty(WorldSerializer.WorldName))
            {
                autosaveTimer += Time.deltaTime;
                if (autosaveTimer >= autosaveInterval)
                {
                    autosaveTimer = 0f;
                    WorldSerializer.Save();
                    Debug.Log("[WorldManager] Autosaved.");
                }
            }
        }

        public float GetSurfaceY(float worldX)
        {
            float cellSize = gridParent.cellSize.x;
            int gridX = Mathf.FloorToInt(worldX / cellSize);

            if (!WorldData.World.SafeCheck(gridX, 0)) return 0f;

            for (int y = worldHeight - 1; y >= 0; y--)
            {
                if (WorldData.World.GetBlockTypes(gridX, y) != BlockType.Air)
                {
                    return y * cellSize;
                }
            }

            return 0f;
        }

        private void GenerateWorld()
        {
            int[] surfaceHeights = new int[worldWidth];

            for (int x = 0; x < worldWidth; x++)
            {
                var noiseValue = 0f;
                noiseValue += Mathf.PerlinNoise((x * tallMountains) + seedOffset, 0) * 1.0f;
                noiseValue += Mathf.PerlinNoise((x * mediumMountains) + seedOffset, 0) * 0.15f;
                noiseValue += Mathf.PerlinNoise((x * smallMountains) + seedOffset, 0) * 0.05f;
                noiseValue /= 1.2f;
                noiseValue = Mathf.Lerp(0.3f, 0.8f, noiseValue);

                var groundLevel = (int)(noiseValue * worldHeight * 0.75f);
                surfaceHeights[x] = groundLevel;

                for (int y = 0; y < worldHeight; y++)
                {
                    if (y > groundLevel)
                    {
                        WorldData.World.SetBlockType(x, y, BlockType.Air);
                    }
                    else
                    {
                        int depth = groundLevel - y;
                        BlockType blockType = GetUndergroundBlock(x, y, depth);
                        WorldData.World.SetBlockType(x, y, blockType);
                    }
                }
            }

            int[,] caveMap = new int[worldWidth, worldHeight];

            for (int x = 0; x < worldWidth; x++)
            {
                float caveEntranceNoise = Mathf.PerlinNoise(x * 0.04f + seedOffset, seedOffset);
                bool isCaveEntranceZone = caveEntranceNoise > 0.75f;

                for (int y = 0; y < worldHeight; y++)
                {
                    if (y > surfaceHeights[x]) continue;

                    float depthFromSurface = surfaceHeights[x] - y;
                    float fillProb = 0.54f;

                    if (depthFromSurface < 15 && !isCaveEntranceZone)
                    {
                        float t = depthFromSurface / 15f;
                        fillProb = Mathf.Lerp(0.90f, 0.54f, t);
                    }

                    caveMap[x, y] = (UnityEngine.Random.value < fillProb) ? 1 : 0;
                }
            }

            int smoothingIterations = 7;
            for (int i = 0; i < smoothingIterations; i++)
            {
                int[,] newCaveMap = new int[worldWidth, worldHeight];
                for (int x = 0; x < worldWidth; x++)
                {
                    for (int y = 0; y < worldHeight; y++)
                    {
                        int neighborWallCount = GetSurroundingWallCount(x, y, caveMap);

                        if (neighborWallCount > 4) newCaveMap[x, y] = 1;
                        else if (neighborWallCount < 4) newCaveMap[x, y] = 0;
                        else newCaveMap[x, y] = caveMap[x, y];
                    }
                }

                caveMap = newCaveMap;
            }

            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    if (caveMap[x, y] == 0)
                    {
                        if (WorldData.World.GetBlockTypes(x, y) != BlockType.Bedrock)
                        {
                            WorldData.World.SetBlockType(x, y, BlockType.Air);
                        }
                    }
                }
            }

            for (int x = 0; x < worldWidth; x++)
            {
                bool foundSurface = false;
                int localDirtDepth = Mathf.FloorToInt(Mathf.PerlinNoise(x * 0.1f + seedOffset, seedOffset) * 5f) +
                                     dirtLayerThickness;
                float baseBiomeNoise = Mathf.PerlinNoise(x * 0.02f + seedOffset, seedOffset * 1.5f);

                for (int y = worldHeight - 1; y >= 0; y--)
                {
                    BlockType currentBlock = WorldData.World.GetBlockTypes(x, y);

                    if (currentBlock != BlockType.Air && !foundSurface)
                    {
                        foundSurface = true;

                        float surfaceWobble = (Mathf.PerlinNoise(x * 0.2f + seedOffset, y * 0.2f + seedOffset) - 0.5f) *
                                              0.15f;
                        float surfaceBiome = baseBiomeNoise + surfaceWobble;

                        BlockType topBlock = BlockType.Grass;
                        if (surfaceBiome < 0.33f) topBlock = BlockType.SurfaceSand;
                        else if (surfaceBiome > 0.66f) topBlock = BlockType.SurfaceClay;

                        WorldData.World.SetBlockType(x, y, topBlock);

                        for (int d = 1; d <= localDirtDepth; d++)
                        {
                            int currentY = y - d;
                            if (currentY >= 0 && WorldData.World.GetBlockTypes(x, currentY) != BlockType.Air)
                            {
                                float subWobble =
                                    (Mathf.PerlinNoise(x * 0.2f + seedOffset, currentY * 0.2f + seedOffset) - 0.5f) *
                                    0.15f;
                                float subBiome = baseBiomeNoise + subWobble;

                                BlockType subBlock = BlockType.Dirt;
                                if (subBiome < 0.33f) subBlock = BlockType.Sand;
                                else if (subBiome > 0.66f) subBlock = BlockType.Clay;

                                WorldData.World.SetBlockType(x, currentY, subBlock);
                            }
                        }
                    }
                }
            }
        }

        private int GetSurroundingWallCount(int gridX, int gridY, int[,] map)
        {
            int wallCount = 0;
            for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
            {
                for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
                {
                    if (neighborX < 0 || neighborX >= worldWidth || neighborY < 0 || neighborY >= worldHeight)
                    {
                        wallCount++;
                    }
                    else if (neighborX != gridX || neighborY != gridY)
                    {
                        wallCount += map[neighborX, neighborY];
                    }
                }
            }

            return wallCount;
        }

        private BlockType GetUndergroundBlock(int x, int y, int depth)
        {
            if (y <= 3)
            {
                if (y == 0 || UnityEngine.Random.value > 0.4f) return BlockType.Bedrock;
            }

            int deepslateHeight = 30;
            BlockType baseBlock = BlockType.Stone;

            if (y <= deepslateHeight)
            {
                if (y < deepslateHeight - 5 || UnityEngine.Random.value > 0.5f)
                {
                    baseBlock = BlockType.Slate;
                }
            }

            float patchScale = 0.08f;
            float patchNoise = Mathf.PerlinNoise((x * patchScale) + seedOffset, (y * patchScale) + seedOffset);

            if (y > 3)
            {
                if (patchNoise > 0.85f) return BlockType.Dirt;
                if (patchNoise < 0.15f) return BlockType.Gravel;
                if (patchNoise > 0.75f && patchNoise <= 0.85f) return BlockType.Sand;
                if (patchNoise >= 0.15f && patchNoise < 0.25f) return BlockType.Clay;
            }

            if (baseBlock == BlockType.Stone || baseBlock == BlockType.Slate)
            {
                float oreChance = Random.value;

                if (oreChance > 0.96f)
                {
                    if (depth > 60)
                    {
                        float gemChance = Random.value;
                        if (gemChance > 0.8f) return BlockType.Ruby;
                        if (gemChance > 0.6f) return BlockType.Sapphire;
                        if (gemChance > 0.4f) return BlockType.Emerald;
                        if (gemChance > 0.2f) return BlockType.Onyx;
                        return BlockType.Topaz;
                    }

                    if (depth > 40)
                    {
                        return (Random.value > 0.5f) ? BlockType.Iron : BlockType.Coal;
                    }

                    if (depth > 15)
                    {
                        return (Random.value > 0.5f) ? BlockType.Copper : BlockType.Tin;
                    }

                    return BlockType.Coal;
                }
            }

            return baseBlock;
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

                    bool isSurface = groundBlock == BlockType.Grass || groundBlock == BlockType.SurfaceSand ||
                                     groundBlock == BlockType.SurfaceClay;
                    bool isUnderground = groundBlock == BlockType.Stone || groundBlock == BlockType.Slate;

                    if (!isSurface && !isUnderground) continue;

                    bool isBarrenBlock = groundBlock == BlockType.SurfaceSand || groundBlock == BlockType.Sand ||
                                         groundBlock == BlockType.Stone || groundBlock == BlockType.Slate;

                    bool hasSkyAccess = true;
                    for (int checkY = y + 1; checkY < worldHeight; checkY++)
                    {
                        if (WorldData.World.GetBlockTypes(x, checkY) != BlockType.Air)
                        {
                            hasSkyAccess = false;
                            break;
                        }
                    }

                    List<Prop> validProps = new List<Prop>();
                    float totalWeight = 0;

                    foreach (var p in props)
                    {
                        if (p.isFromSurface != isSurface) continue;
                        if (p.hasPriority != isPriorityPass) continue;
                        if (p.isFromSurface && !hasSkyAccess) continue;
                        if (p.type.ToString() == "Sulphur") continue;

                        if (isBarrenBlock && p.type != PropType.StoneProp) continue;

                        bool hasValidBlock = false;
                        if (p.allowedGroundBlocks == null || p.allowedGroundBlocks.Length == 0)
                        {
                            hasValidBlock = true;
                        }
                        else
                        {
                            foreach (var allowed in p.allowedGroundBlocks)
                            {
                                if (groundBlock == allowed)
                                {
                                    hasValidBlock = true;
                                    break;
                                }
                            }
                        }

                        if (!hasValidBlock) continue;

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
            if (string.IsNullOrEmpty(seed)) return Random.Range(-100000f, 100000f);
            System.Random prng = new System.Random(seed.GetHashCode());
            return prng.Next(-100000, 100000);
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
                    pRenderer.sortingOrder = -1;

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
            if (!mainCamera) return false;

            if (mainCamera.transform.position != cameraPosition)
            {
                cameraPosition = mainCamera.transform.position;
                return true;
            }

            return false;
        }

        public void UpdateDynamicLighting(int targetX, int targetY)
        {
            int minX = Mathf.Max(0, targetX - lightUpdateRadius);
            int maxX = Mathf.Min(worldWidth - 1, targetX + lightUpdateRadius);
            int minY = 0;
            int maxY = worldHeight - 1;

            for (int i = minX; i <= maxX; i++)
            {
                float currentSunlight = 1.0f;
                for (int j = maxY; j >= 0; j--)
                {
                    BlockType blockType = WorldData.World.GetBlockTypes(i, j);
                    bool air = blockType == BlockType.Air;
                    isAirCache[i, j] = air;

                    if (!air)
                    {
                        currentSunlight *= (1.0f - verticalSolidDecay);
                    }

                    lightMapCache[i, j] = currentSunlight;
                }
            }

            for (int iteration = 0; iteration < lightIterations; iteration++)
            {
                if (iteration % 2 == 0)
                {
                    for (int i = minX; i <= maxX; i++)
                    {
                        for (int j = minY; j <= maxY; j++)
                        {
                            float currentValue = lightMapCache[i, j];
                            float neighbourMax = 0f;

                            if (i > 0 && lightMapCache[i - 1, j] > neighbourMax) neighbourMax = lightMapCache[i - 1, j];
                            if (i < worldWidth - 1 && lightMapCache[i + 1, j] > neighbourMax) neighbourMax = lightMapCache[i + 1, j];
                            if (j > 0 && lightMapCache[i, j - 1] > neighbourMax) neighbourMax = lightMapCache[i, j - 1];
                            if (j < worldHeight - 1 && lightMapCache[i, j + 1] > neighbourMax) neighbourMax = lightMapCache[i, j + 1];

                            float decay = isAirCache[i, j] ? spreadAirDecay : spreadSolidDecay;
                            float spreadValue = neighbourMax * (1.0f - decay);

                            if (spreadValue > currentValue)
                            {
                                lightMapCache[i, j] = spreadValue;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = maxX; i >= minX; i--)
                    {
                        for (int j = maxY; j >= minY; j--)
                        {
                            float currentValue = lightMapCache[i, j];
                            float neighbourMax = 0f;

                            if (i > 0 && lightMapCache[i - 1, j] > neighbourMax) neighbourMax = lightMapCache[i - 1, j];
                            if (i < worldWidth - 1 && lightMapCache[i + 1, j] > neighbourMax) neighbourMax = lightMapCache[i + 1, j];
                            if (j > 0 && lightMapCache[i, j - 1] > neighbourMax) neighbourMax = lightMapCache[i, j - 1];
                            if (j < worldHeight - 1 && lightMapCache[i, j + 1] > neighbourMax) neighbourMax = lightMapCache[i, j + 1];

                            float decay = isAirCache[i, j] ? spreadAirDecay : spreadSolidDecay;
                            float spreadValue = neighbourMax * (1.0f - decay);

                            if (spreadValue > currentValue)
                            {
                                lightMapCache[i, j] = spreadValue;
                            }
                        }
                    }
                }
            }

            float finalMultiplier = 1.0f;
            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    float finalLight = lightMapCache[x, y];
                    WorldData.World.lightValues[x, y] = finalLight;

                    finalLight *= finalMultiplier;
                    finalLight = Mathf.Pow(finalLight, 1.5f);
                    lightmapColors[y * worldWidth + x] = new Color(finalLight, finalLight, finalLight, 1f);
                }
            }

            lightmapTexture.SetPixels(lightmapColors);
            lightmapTexture.Apply();
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
                    bool air = (blockType == BlockType.Air);
                    isAirCache[i, j] = air;

                    if (!air)
                    {
                        currentSunlight *= (1.0f - verticalSolidDecay);
                    }

                    lightMapCache[i, j] = currentSunlight;
                }
            }

            for (int iteration = 0; iteration < lightIterations; iteration++)
            {
                if (iteration % 2 == 0)
                {
                    for (int i = 0; i < width; i++)
                    {
                        for (int j = 0; j < height; j++)
                        {
                            float currentValue = lightMapCache[i, j];
                            float neighbourMax = 0f;

                            if (i > 0 && lightMapCache[i - 1, j] > neighbourMax) neighbourMax = lightMapCache[i - 1, j];
                            if (i < width - 1 && lightMapCache[i + 1, j] > neighbourMax) neighbourMax = lightMapCache[i + 1, j];
                            if (j > 0 && lightMapCache[i, j - 1] > neighbourMax) neighbourMax = lightMapCache[i, j - 1];
                            if (j < height - 1 && lightMapCache[i, j + 1] > neighbourMax) neighbourMax = lightMapCache[i, j + 1];

                            float decay = isAirCache[i, j] ? spreadAirDecay : spreadSolidDecay;
                            float spreadValue = neighbourMax * (1.0f - decay);

                            if (spreadValue > currentValue)
                            {
                                lightMapCache[i, j] = spreadValue;
                            }
                        }
                    }
                }
                else
                {
                    for (int i = width - 1; i >= 0; i--)
                    {
                        for (int j = height - 1; j >= 0; j--)
                        {
                            float currentValue = lightMapCache[i, j];
                            float neighbourMax = 0f;

                            if (i > 0 && lightMapCache[i - 1, j] > neighbourMax) neighbourMax = lightMapCache[i - 1, j];
                            if (i < width - 1 && lightMapCache[i + 1, j] > neighbourMax) neighbourMax = lightMapCache[i + 1, j];
                            if (j > 0 && lightMapCache[i, j - 1] > neighbourMax) neighbourMax = lightMapCache[i, j - 1];
                            if (j < height - 1 && lightMapCache[i, j + 1] > neighbourMax) neighbourMax = lightMapCache[i, j + 1];

                            float decay = isAirCache[i, j] ? spreadAirDecay : spreadSolidDecay;
                            float spreadValue = neighbourMax * (1.0f - decay);

                            if (spreadValue > currentValue)
                            {
                                lightMapCache[i, j] = spreadValue;
                            }
                        }
                    }
                }
            }

            float finalMultiplier = 1.0f;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float finalLight = lightMapCache[x, y];
                    WorldData.World.lightValues[x, y] = finalLight;

                    finalLight *= finalMultiplier;
                    finalLight = Mathf.Pow(finalLight, 1.5f);
                    lightmapColors[y * width + x] = new Color(finalLight, finalLight, finalLight, 1f);
                }
            }
        }

        private void ApplyLightingToTexture()
        {
            float finalMultiplier = 1.0f;
            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    float l = WorldData.World.lightValues[x, y] * finalMultiplier;
                    l = Mathf.Pow(l, 1.5f);
                    int index = y * worldWidth + x;
                    lightmapColors[index] = new Color(l, l, l, 1f);
                }
            }

            lightmapTexture.SetPixels(lightmapColors);
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

        private void SpawnPlayer()
        {
            int spawnX = worldWidth / 2;
            float cellSize = gridParent.cellSize.x;

            for (int y = worldHeight - 1; y >= 0; y--)
            {
                BlockType currentBlock = WorldData.World.GetBlockTypes(spawnX, y);

                if (currentBlock != BlockType.Air)
                {
                    float worldX = (spawnX * cellSize) + (cellSize / 2f);
                    float worldY = ((y + 2) * cellSize);

                    currentSpawnPoint = new Vector3(worldX, worldY, 0);

                    GameObject spawnedPlayer = Instantiate(playerPrefab, currentSpawnPoint, Quaternion.identity);

                    if (virtualCamera != null)
                    {
                        virtualCamera.Follow = spawnedPlayer.transform;
                    }

                    break;
                }
            }
        }

        public void SetSpawnPoint(Vector3 newSpawnPoint)
        {
            currentSpawnPoint = newSpawnPoint;
        }

        public void RespawnPlayer(GameObject playerObject)
        {
            playerObject.transform.position = currentSpawnPoint;

            mainCamera.transform.position =
                new Vector3(currentSpawnPoint.x, currentSpawnPoint.y, mainCamera.transform.position.z);
            cameraPosition = mainCamera.transform.position;

            UpdateChunks();

            if (virtualCamera != null)
            {
                virtualCamera.PreviousStateIsValid = false;
            }

            PlayerMovement.Singleton.enableTimer = 0.5f;
            UIController.Singleton.UpdateHealth(1, 1);
        }

        public bool TrySetSpawnFromAnchor(Vector3 interactWorldPosition)
        {
            float cellSize = gridParent.cellSize.x;
            int gridX = Mathf.FloorToInt(interactWorldPosition.x / cellSize);
            int gridY = Mathf.FloorToInt(interactWorldPosition.y / cellSize);

            if (!WorldData.World.SafeCheck(gridX, gridY)) return false;

            if (WorldData.World.GetBlockTypes(gridX, gridY) == BlockType.spawnAnchor)
            {
                float spawnX = (gridX * cellSize) + (cellSize / 2f);
                float spawnY = (gridY + 1) * cellSize;

                SetSpawnPoint(new Vector3(spawnX, spawnY, 0));
                return true;
            }

            return false;
        }

        private void OnApplicationQuit()
        {
            if (!string.IsNullOrEmpty(WorldSerializer.WorldName))
            {
                WorldSerializer.Save();
                Debug.Log("[WorldManager] Saved on quit.");
            }
        }
    }
}
