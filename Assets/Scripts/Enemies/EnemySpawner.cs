using System.Collections.Generic;
using Data;
using UnityEngine;
using World;

namespace Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [System.Serializable]
        public class EnemySpawnData
        {
            public string name;
            public GameObject enemyPrefab;
            public bool isCaveEnemy;
            public bool isFlying;
            [Range(1f, 100f)] public float spawnWeight = 10f;
        }

        #region Singleton setup

        public static EnemySpawner Singleton;

        private void SetupSingleton()
        {
            #region SetupSingleton

            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
                return;
            }

            Singleton = this;

            #endregion
        }

        #endregion

        [Header("Dependencies")]
        [SerializeField] private DayNightCycle dayNightCycle;
        [SerializeField] private float cellSize = 0.5f;
        private Transform player;

        [Header("Spawn Settings")]
        public List<EnemySpawnData> enemies;
        public float daySpawnInterval = 15f;
        public float nightSpawnInterval = 4f;
        public int maxActiveEnemies = 15;

        [Header("Location & Cleanup Settings")]
        public float minSpawnDistance = 10f;
        public float maxSpawnDistance = 25f;
        public float despawnDistance = 40f;
        public float caveTransitionY = 15f;

        private float spawnTimer;
        private List<GameObject> activeEnemies = new List<GameObject>();

        /// <summary>Ran by unity on load</summary>
        private void Awake()
        {
            #region Awake
            SetupSingleton();
            #endregion
        }

        private void Update()
        {
            HandleEnemyDespawning();

            if (player == null)
            {
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null) player = playerObj.transform;
                else return;
            }

            if (activeEnemies.Count >= maxActiveEnemies) return;

            spawnTimer -= Time.deltaTime;

            if (spawnTimer <= 0f)
            {
                AttemptSpawn();
                spawnTimer = dayNightCycle.IsNight ? nightSpawnInterval : daySpawnInterval;
            }
        }

        private void HandleEnemyDespawning()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] == null)
                {
                    activeEnemies.RemoveAt(i);
                    continue;
                }

                float distanceToPlayer = Vector2.Distance(player.position, activeEnemies[i].transform.position);
                if (distanceToPlayer > despawnDistance)
                {
                    Destroy(activeEnemies[i]);
                    activeEnemies.RemoveAt(i);
                }
            }
        }

        private void AttemptSpawn()
        {
            // 1. Check if WorldManager exists to prevent null references
            if (WorldManager.Instance == null) return;

            // 2. Get the dynamic surface level for the player's current X position
            float dynamicSurfaceY = WorldManager.Instance.GetSurfaceY(player.position.x);

            // 3. Determine if in cave.
            // Note: I added a small offset (- 2f) so they don't immediately count as "in cave"
            // the millisecond they step below the highest grass block. Tweak this as needed!
            bool playerInCave = player.position.y < (dynamicSurfaceY - 2f);

            List<EnemySpawnData> validEnemies = enemies.FindAll(e => e.isCaveEnemy == playerInCave);

            if (validEnemies.Count == 0) return;

            EnemySpawnData selectedEnemy = GetRandomEnemy(validEnemies);
            if (selectedEnemy == null) return;

            for (int i = 0; i < 10; i++)
            {
                Vector2 spawnPos = GetRandomSpawnPosition(selectedEnemy.isFlying);

                int gridX = Mathf.FloorToInt(spawnPos.x / cellSize);
                int gridY = Mathf.FloorToInt(spawnPos.y / cellSize);

                if (gridX < 0 || gridX >= WorldData.World.width || gridY <= 0 || gridY >= WorldData.World.height)
                    continue;

                float snappedX = (gridX * cellSize) + (cellSize / 2f);

                if (selectedEnemy.isFlying)
                {
                    if (CheckHasSpace(gridX, gridY))
                    {
                        float snappedY = (gridY * cellSize) + (cellSize / 2f);
                        InstantiateEnemy(selectedEnemy.enemyPrefab, new Vector2(snappedX, snappedY));
                        return;
                    }
                }
                else
                {
                    int foundY = -1;

                    if (WorldData.World.GetBlockTypes(gridX, gridY) != BlockType.Air)
                    {
                        for (int yOffset = 0; yOffset < 20; yOffset++)
                        {
                            int checkY = gridY + yOffset;
                            if (checkY >= WorldData.World.height - 2) break;

                            if (CheckHasSpace(gridX, checkY) &&
                                WorldData.World.GetBlockTypes(gridX, checkY - 1) != BlockType.Air)
                            {
                                foundY = checkY;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int yOffset = 0; yOffset < 20; yOffset++)
                        {
                            int checkY = gridY - yOffset;
                            if (checkY <= 0) break;

                            if (CheckHasSpace(gridX, checkY) &&
                                WorldData.World.GetBlockTypes(gridX, checkY - 1) != BlockType.Air)
                            {
                                foundY = checkY;
                                break;
                            }
                        }
                    }

                    if (foundY != -1)
                    {
                        float snappedY = (foundY * cellSize) + cellSize;
                        Vector2 snappedPos = new Vector2(snappedX, snappedY);
                        InstantiateEnemy(selectedEnemy.enemyPrefab, snappedPos);
                        return;
                    }
                }
            }
        }

        private bool CheckHasSpace(int x, int startY)
        {
            for (int h = 0; h < 2; h++)
            {
                if (WorldData.World.GetBlockTypes(x, startY + h) != BlockType.Air)
                {
                    return false;
                }
            }
            return true;
        }

        private void InstantiateEnemy(GameObject prefab, Vector2 position)
        {
            GameObject newEnemy = Instantiate(prefab, position, Quaternion.identity);
            activeEnemies.Add(newEnemy);
        }

        private Vector2 GetRandomSpawnPosition(bool isFlying)
        {
            if (isFlying)
            {
                float randomX = Random.Range(-maxSpawnDistance, maxSpawnDistance);
                float randomY = Random.Range(minSpawnDistance, maxSpawnDistance);
                return new Vector2(player.position.x + randomX, player.position.y + randomY);
            }

            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(minSpawnDistance, maxSpawnDistance);
            return (Vector2)player.position + (randomDirection * randomDistance);
        }

        private EnemySpawnData GetRandomEnemy(List<EnemySpawnData> pool)
        {
            float totalWeight = 0;
            foreach (var e in pool) totalWeight += e.spawnWeight;

            float roll = Random.Range(0f, totalWeight);
            float currentWeight = 0;

            foreach (var e in pool)
            {
                currentWeight += e.spawnWeight;
                if (roll <= currentWeight) return e;
            }
            return null;
        }
    }
}
