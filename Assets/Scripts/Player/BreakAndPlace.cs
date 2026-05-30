using System.Collections.Generic;
using Chunks;
using Data;
using UnityEngine;
using Scriptable_Objects_Scripts;
using World;
using Items;
using Items.Overlays;

namespace Player
{
    public class BreakAndPlace : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Controls")]
        public int reachDistance = 5;

        [Header("Prop Attack Hitbox (Matched to PlayerAttack)")]
        [SerializeField] private Vector2 attackOffset = new Vector2(1f, 0f);
        [SerializeField] private Vector2 attackBoxSize = new Vector2(1.5f, 1.5f);

        private const float CellSize = 0.5f;

        [Header("Mining Data")]
        private Vector2Int currentMineTarget = new Vector2Int(-999, -999);
        private float currentBlockDamage = 3f;
        private Collider2D playerCollider;

        [Header("Prop Attack Data")]
        private Vector2Int currentPropTarget = new Vector2Int(-999, -999);
        private float currentPropDamage = 0f;

        [Header("Prefabs")]
        [SerializeField] private GameObject itemEntityPrefab;

        private Vector2 cachedTargetPosition;
        private readonly Collider2D[] hitResults = new Collider2D[10];

        private void Awake()
        {
            playerCollider = GetComponent<BoxCollider2D>();
        }

        private void OnEnable()
        {
            if (playerMovement != null)
            {
                playerMovement.OnAttackPerformed += OnAttackInputReceived;
                playerMovement.OnMinePerformed += OnMineInputReceived;
            }
        }

        private void OnDisable()
        {
            if (playerMovement != null)
            {
                playerMovement.OnAttackPerformed -= OnAttackInputReceived;
                playerMovement.OnMinePerformed -= OnMineInputReceived;
            }
        }

        private void Update()
        {
            // Input.GetMouseButtonDown(2) is the Middle Mouse Button (Mouse Wheel Click).
            // (If you ever want to change this to Right-Click, just change the 2 to a 1!)
            if (Input.GetMouseButtonDown(2))
            {
                Vector3 realScreenPos = Input.mousePosition;
                float camZ = Mathf.Abs(Camera.main.transform.position.z);
                Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(realScreenPos.x, realScreenPos.y, camZ));

                ItemStack item = Inventory.Singleton.hand;

                if (item != null && item.data.isPlacable)
                {
                    TryPlaceBlock(item, worldMousePos);
                }
            }
        }

        #region Input Listeners (Trigger Animations Here)

        private void OnAttackInputReceived(Vector2 ignoredMousePosition)
        {
            Vector3 realScreenPos = Input.mousePosition;
            float camZ = Mathf.Abs(Camera.main.transform.position.z);
            Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(realScreenPos.x, realScreenPos.y, camZ));

            BlockType clickedBlock = GetClickedBlock(worldMousePos, out ulong blockId);

            if (clickedBlock == BlockType.Entity)
            {
                UIController.Singleton.OpenOverlay(blockId);
                return;
            }

            cachedTargetPosition = worldMousePos;
        }

        private void OnMineInputReceived(Vector2 ignoredMousePosition)
        {
            // Now this ONLY caches the target for the mining animation event.
            // No placing logic happens here anymore.
            Vector3 realScreenPos = Input.mousePosition;
            float camZ = Mathf.Abs(Camera.main.transform.position.z);
            Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(realScreenPos.x, realScreenPos.y, camZ));

            cachedTargetPosition = worldMousePos;
        }

        #endregion

        #region Animation Events (Public Methods)

        public void ExecuteAttack()
{
    int direction = cachedTargetPosition.x < transform.position.x ? -1 : 1;
    Vector2 attackCenter = (Vector2)transform.position + new Vector2(attackOffset.x * direction, attackOffset.y);

    int hitCount = Physics2D.OverlapBoxNonAlloc(attackCenter, attackBoxSize, 0f, hitResults);

    for (int i = 0; i < hitCount; i++)
    {
        if (hitResults[i].TryGetComponent(out UnityEngine.Tilemaps.Tilemap tilemap))
        {
            Vector3Int hitCell = tilemap.WorldToCell(attackCenter);
            bool foundProp = false;

            for (int yOffset = 0; yOffset >= -6; yOffset--)
            {
                for (int xOffset = -1; xOffset <= 1; xOffset++)
                {
                    int checkX = hitCell.x + xOffset;
                    int checkY = hitCell.y + yOffset;

                    if (!WorldData.World.SafeCheck(checkX, checkY)) continue;

                    PropType hitType = WorldData.World.GetPropType(checkX, checkY);

                    if (hitType != PropType.None)
                    {
                        if (currentPropTarget.x != checkX || currentPropTarget.y != checkY)
                        {
                            currentPropTarget = new Vector2Int(checkX, checkY);
                            currentPropDamage = 0f;
                        }

                        Prop propHitData = WorldData.PropDictionary[hitType];
                        float hitPower = Equipment.Singleton.GetMiningPower();

                        currentPropDamage += hitPower;

                        Debug.Log($"[Prop Attack] Hitting {hitType} at ({checkX}, {checkY}) | Dmg: {currentPropDamage}/{propHitData.hardness}");

                        if (currentPropDamage >= propHitData.hardness)
                        {
                            BreakProp(hitType, checkX, checkY);
                            currentPropDamage = 0f;
                        }

                        foundProp = true;
                        break;
                    }
                }
                if (foundProp) break;
            }
        }
    }
}

        public void ExecuteMining()
        {
            Vector3 realScreenPos = Input.mousePosition;
            float camZ = Mathf.Abs(Camera.main.transform.position.z);
            Vector2 actualMouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(realScreenPos.x, realScreenPos.y, camZ));

            Vector2 playerCenter = playerCollider != null ? (Vector2)playerCollider.bounds.center : (Vector2)transform.position;
            float distance = Vector2.Distance(actualMouseWorldPos, playerCenter);

            if (distance > reachDistance)
            {
                Debug.Log($"[Mining] Failed: Distance {distance} | Target: {actualMouseWorldPos} | Player: {playerCenter}");
                return;
            }

            int x = Mathf.FloorToInt(actualMouseWorldPos.x / CellSize);
            int y = Mathf.FloorToInt(actualMouseWorldPos.y / CellSize);

            if (currentMineTarget.x != x || currentMineTarget.y != y)
            {
                currentMineTarget = new Vector2Int(x, y);
                currentBlockDamage = 0f;
            }

            BlockType clickedBlock = WorldData.World.GetBlockTypes(x, y);

            if (clickedBlock == BlockType.Air)
            {
                Debug.Log($"[Mining] Failed: Clicked on Air at Grid ({x}, {y})");
                return;
            }

            float targetHardness = WorldData.BlockDictionary[clickedBlock].hardness;
            float miningPower = Equipment.Singleton.GetMiningPower();

            Debug.Log($"[Mining] Hitting {clickedBlock} at ({x}, {y}) | Power: {miningPower} | Dmg: {currentBlockDamage}/{targetHardness}");

            currentBlockDamage += miningPower;

            if (currentBlockDamage >= targetHardness)
            {
                Debug.Log($"[Mining] SUCCESS! Broke {clickedBlock}.");
                BreakBlock(x, y, clickedBlock);
                currentBlockDamage = 0f;

                if (clickedBlock == BlockType.Entity)
                {
                    UIController.Singleton.DestroyEntity(BlockIdUtils.From(x, y));
                }
            }
        }
        #endregion

        #region Core Logic

        private BlockType GetClickedBlock(Vector2 mousePos, out ulong blockId)
        {
            int x = Mathf.FloorToInt(mousePos.x / CellSize);
            int y = Mathf.FloorToInt(mousePos.y / CellSize);

            blockId = BlockIdUtils.From(x, y);
            return WorldData.World.GetBlockTypes(x, y);
        }

        private bool TryPlaceBlock(ItemStack item, Vector2 mouseWorldPosition)
        {
            Vector2 playerCenter = playerCollider != null ? (Vector2)playerCollider.bounds.center : (Vector2)transform.position;

            int x = Mathf.FloorToInt(mouseWorldPosition.x / CellSize);
            int y = Mathf.FloorToInt(mouseWorldPosition.y / CellSize);

            Vector2 cellWorldPos = new Vector2(x, y) * CellSize + Vector2.one * (CellSize / 2f);
            float distance = Vector2.Distance(cellWorldPos, playerCenter);

            if (distance > reachDistance) return true;

            BlockType clickedBlock = WorldData.World.GetBlockTypes(x, y);
            if (clickedBlock != BlockType.Air) return true;

            if (distance < 0.5f) return true;

            WorldData.World.SetBlockType(x, y, item.data.blockType);
            UpdateChunkVisuals(x, y);

            Inventory.Singleton.RemoveFromHand();

            if (item.data.blockType == BlockType.Entity)
            {
                UIController.Singleton.CreateOverlay(BlockIdUtils.From(x, y), item.data.overlayType);
            }
            return false;
        }

        private void BreakBlock(int x, int y, BlockType type)
        {
            Vector2 spawnPosition = new Vector2(x * CellSize + 0.25f, y * CellSize + 0.25f);

            ItemData blockData = WorldData.BlockDictionary[type];

            // Check if we have specific drops set up for this block
            if (blockData.drops != null && blockData.drops.Count > 0)
            {
                foreach (Drop drop in blockData.drops)
                {
                    bool doesDrop = Random.Range(0, 100) <= drop.dropChance;
                    if (!doesDrop) continue;

                    int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);

                    for (int i = 0; i < amount; i++)
                    {
                        SpawnLoot(drop.item, spawnPosition);
                    }
                }
            }
            else
            {
                // Fallback: If no drops are defined in the inspector, just drop the block itself
                SpawnLoot(blockData, spawnPosition);
            }

            WorldData.World.SetBlockType(x, y, BlockType.Air);
            UpdateChunkVisuals(x, y);
        }

        private void BreakProp(PropType hitType, int checkX, int checkY)
        {
            Prop propHitData = WorldData.PropDictionary[hitType];
            Vector2 spawnPosition = new Vector2(checkX, checkY) * CellSize + new Vector2(0.25f, 0.25f);

            foreach (Drop drop in propHitData.drops)
            {
                bool doesDrop = Random.Range(0, 100) <= drop.dropChance;
                if (!doesDrop) continue;

                int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);

                for (int i = 0; i < amount; i++)
                {
                    SpawnLoot(drop.item, spawnPosition);
                }
            }

            WorldData.World.SetPropType(checkX, checkY, PropType.None);
            UpdateChunkVisuals(checkX, checkY);
        }

        private void SpawnLoot(ItemData itemData, Vector2 position)
        {
            if (itemData == null || itemEntityPrefab == null) return;

            GameObject droppedItem = Instantiate(itemEntityPrefab, position, Quaternion.identity);
            Vector2 randomOffset = new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
            droppedItem.transform.position += (Vector3)randomOffset;

            if (droppedItem.TryGetComponent(out ItemEntity entity))
            {
                entity.Initialize(itemData);
            }
        }

        private void UpdateChunkVisuals(int x, int y)
        {
            int chunkX = Mathf.FloorToInt((float)x / Chunk.ChunkSize);
            int chunkY = Mathf.FloorToInt((float)y / Chunk.ChunkSize);

            WorldManager.Instance.chunks[chunkX, chunkY].UpdateTile(x, y);
        }

        #endregion
    }
}
