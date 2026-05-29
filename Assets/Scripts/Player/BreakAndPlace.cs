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

        [Header("Prefabs")]
        [SerializeField] private GameObject itemEntityPrefab;

        // Caches the mouse position from the event so the animation event knows where to strike
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

        #region Input Listeners (Trigger Animations Here)

        private void OnAttackInputReceived(Vector2 mousePosition)
        {
            BlockType clickedBlock = GetClickedBlock(mousePosition, out ulong blockId);

            if (clickedBlock == BlockType.Entity)
            {
                UIController.Singleton.OpenOverlay(blockId);
                return;
            }

            cachedTargetPosition = mousePosition;

        }

        private void OnMineInputReceived(Vector2 mousePosition)
        {
            ItemStack item = Inventory.Singleton.hand;

            if (item != null && item.data.isPlacable)
            {
                bool shouldMine = TryPlaceBlock(item, mousePosition);
                if (!shouldMine) return;
            }
            cachedTargetPosition = mousePosition;

        }

        #endregion

        #region Animation Events (Public Methods)

        /// <summary>
        /// ANIMATION EVENT: Call this exactly when the tool visually hits during the Attack animation.
        /// </summary>
        public void ExecuteAttack()
        {
            // Matched to how PlayerAttack calculates its hitbox
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
                                BreakProp(hitType, checkX, checkY);
                                foundProp = true;
                                break;
                            }
                        }
                        if (foundProp) break;
                    }
                }
            }
        }

        /// <summary>
        /// ANIMATION EVENT: Call this exactly when the pickaxe visually hits during the Mine animation.
        /// </summary>
        public void ExecuteMining()
        {
            Vector2 playerCenter = playerCollider != null ? (Vector2)playerCollider.bounds.center : (Vector2)transform.position;
            float distance = Vector2.Distance(cachedTargetPosition, playerCenter);

            if (distance > reachDistance) return;

            int x = Mathf.FloorToInt(cachedTargetPosition.x / CellSize);
            int y = Mathf.FloorToInt(cachedTargetPosition.y / CellSize);

            if (currentMineTarget.x != x || currentMineTarget.y != y)
            {
                currentMineTarget = new Vector2Int(x, y);
                currentBlockDamage = 0f;
            }

            BlockType clickedBlock = WorldData.World.GetBlockTypes(x, y);

            if (clickedBlock == BlockType.Air) return;

            float targetHardness = WorldData.BlockDictionary[clickedBlock].hardness;
            float miningPower = Equipment.Singleton.GetMiningPower();

            currentBlockDamage += miningPower;

            if (currentBlockDamage >= targetHardness)
            {
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
            Vector2 playerCenter = transform.position;

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

            // Using your static WorldData dictionaries which are populated by WorldManager
            ItemData itemToDrop = WorldData.BlockDictionary[type];
            SpawnLoot(itemToDrop, spawnPosition);

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
