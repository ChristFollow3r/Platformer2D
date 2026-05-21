using System.Collections.Generic;
using Chunks;
using Data;
using UnityEngine;
using Scriptable_Objects_Scripts;
using World;
using Items;

namespace Player
{
    public class BreakAndPlace : MonoBehaviour
    {
        [Header("Drops")]
        [SerializeField] private GameObject dropPrefab;

        [Header("Refs")]
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Controls")]
        public int reachDistance;
        // [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private Vector2 attackBoxSize = new Vector2(1.5f, 1.5f);

        [Header("Mining Data")]
        private Vector2Int currentMineTarget = new Vector2Int(-999, -999);
        private float currentBlockDamage = 0f; // This should be per block, not global (Dmg resets)


        private Camera mainCamera;
        private Collider2D playerCollider;

        private void OnEnable()
        {
            if (playerMovement is not null)
            {
                playerMovement.OnAttackPerformed += HandleAttacking;
                playerMovement.OnMinePerformed += OnRightClick;
            }

        }

        private void OnDisable()
        {
            if (playerMovement != null)
            {
                playerMovement.OnAttackPerformed -= HandleAttacking;
                playerMovement.OnMinePerformed -= OnRightClick;
            }
        }

        private void Awake()
        {
            mainCamera = Camera.main;
            playerCollider = GetComponent<BoxCollider2D>();
        }

        private void HandleAttacking(Vector2 mouseWorldPosition)
        {
            BlockType clickedBlock = GetClickedBlock(mouseWorldPosition, out ulong blockId);

            if (clickedBlock == BlockType.Entity)
            {
                UIController.Singleton.OpenOverlay(blockId);
                return;
            }
            StartCoroutine(DelayedAttackRoutine(mouseWorldPosition));
        }


        private void OnRightClick(Vector2 mouseWorldPosition)
        {
            ItemStack item = Inventory.Singleton.hand;
            if (item != null && item.data.isPlacable)
            {
                bool shouldMine = PlaceBlock(item, mouseWorldPosition);
                if (!shouldMine) return;
            }

            HandleMining(mouseWorldPosition);
        }

        private BlockType GetClickedBlock(Vector2 mousePos, out ulong blockId)
        {
            #region GetCkicledBlock
            float cellSize = 0.5f;
            int x = Mathf.FloorToInt(mousePos.x / cellSize);
            int y = Mathf.FloorToInt(mousePos.y / cellSize);

            blockId = BlockIdUtils.From(x, y);

            return WorldData.World.GetBlockTypes(x, y);
            #endregion
        }


        private bool PlaceBlock(ItemStack item, Vector2 mouseWorldPosition)
        {
            Vector2 playerCenter = transform.position;

            float cellSize = 0.5f;
            int x = Mathf.FloorToInt(mouseWorldPosition.x / cellSize);
            int y = Mathf.FloorToInt(mouseWorldPosition.y / cellSize);
            Vector2 cellWorldPos = new Vector2(x, y) * cellSize + Vector2.one * (cellSize / 2f);
            float distance = Vector2.Distance(cellWorldPos, playerCenter);

            if (distance > reachDistance) return true;
            BlockType clickedBlock = WorldData.World.GetBlockTypes(x, y);

            if (clickedBlock != BlockType.Air) return true;

            if (distance < 0.5f) return true;

            WorldData.World.SetBlockType(x, y, item.data.blockType);
            int chunkX = x / Chunk.ChunkSize;
            int chunkY = y / Chunk.ChunkSize;
            WorldManager.Instance.chunks[chunkX, chunkY].UpdateTile(x, y);
            Inventory.Singleton.RemoveFromHand();

            if (item.data.blockType == BlockType.Entity)
            {
                UIController.Singleton.CreateOverlay(BlockIdUtils.From(x, y), item.data.overlayType);
            }
            return false;
        }

        private void HandleMining(Vector2 mouseWorldPosition)
        {
            Vector2 playerCenter = playerCollider.bounds.center;
            float distance = Vector2.Distance(mouseWorldPosition, playerCenter);

            if (distance > reachDistance) return;

            float cellSize = 0.5f;
            int x = Mathf.FloorToInt(mouseWorldPosition.x / cellSize);
            int y = Mathf.FloorToInt(mouseWorldPosition.y / cellSize);

            if (currentMineTarget.x != x || currentMineTarget.y != y)
            {
                currentMineTarget = new Vector2Int(x, y);
                currentBlockDamage = 0f;
            }

            BlockType clickedBlock = WorldData.World.GetBlockTypes(x, y);

            if (clickedBlock == BlockType.Air) return;

            float targetHardness = WorldData.BlockDictionary[clickedBlock].hardness;
            float miningPower = 1f; // TODO: Base this on the player upgrade

            currentBlockDamage += miningPower;

            if (currentBlockDamage >= targetHardness)
            {
                // TODO: break block, drop shit, add sound, add particles
                BreakBlock(x, y, clickedBlock);
                currentBlockDamage = 0f;
                if (clickedBlock == BlockType.Entity)
                {
                    UIController.Singleton.DestroyEntity(BlockIdUtils.From(x, y));
                }
            }

        }

        private void BreakBlock(int x, int y, BlockType type)
        {
            float cellSize = 0.5f;
            Vector2 spawnPosition = new Vector2(x * cellSize + 0.25f, y * cellSize + 0.25f);
            GameObject dropGO = Instantiate(dropPrefab, spawnPosition, Quaternion.identity);
            DropComponent dropComp = dropGO.GetComponent<DropComponent>();
            dropComp.player = transform;
            dropComp.SetItem(WorldData.BlockDictionary[type]);
            Destroy(dropGO, 300f);

            WorldData.World.SetBlockType(x, y, BlockType.Air);
            int chunkX = x / Chunk.ChunkSize;
            int chunkY = y / Chunk.ChunkSize;
            WorldManager.Instance.chunks[chunkX, chunkY].UpdateTile(x, y);
        }

        private System.Collections.IEnumerator DelayedAttackRoutine(Vector2 mouseWorldPosition)
        {
            yield return new WaitForSeconds(0.30f);

            Vector2 playerCenter = transform.position;
            Vector2 attackDirection = new Vector2(mouseWorldPosition.x - playerCenter.x, 0).normalized;
            Vector2 attackCenter = playerCenter + attackDirection * (attackBoxSize * 0.5f);

            Collider2D[] hitObjects = Physics2D.OverlapBoxAll(attackCenter, attackBoxSize, 0f);

            #region HitBoxDebug
            Vector2 min = attackCenter - (attackBoxSize * 0.5f);
            Vector2 max = attackCenter + (attackBoxSize * 0.5f);
            Vector2 topLeft = new Vector2(min.x, max.y);
            Vector2 bottomRight = new Vector2(max.x, min.y);

            // Draw the 4 sides of the box
            Debug.DrawLine(min, topLeft, Color.magenta, 1f); // Left side
            Debug.DrawLine(topLeft, max, Color.magenta, 1f); // Top side
            Debug.DrawLine(max, bottomRight, Color.magenta, 1f); // Right side
            Debug.DrawLine(bottomRight, min, Color.magenta, 1f); // Bottom side
            #endregion

            foreach (Collider2D hitObject in hitObjects)
            {
                if (hitObject.TryGetComponent(out UnityEngine.Tilemaps.Tilemap tilemap))
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
                                Prop propHitData = WorldData.PropDictionary[hitType];

                                // TODO: Audio source shit
                                // TODO: Particles shit

                                Vector2 spawnPosition = new Vector2(checkX, checkY) * 0.5f + new Vector2(0.25f, 0.25f);

                                foreach (Drop drop in propHitData.drops)
                                {
                                    bool doesDrop = Random.Range(0, 100) <= drop.dropChance;
                                    if (!doesDrop) continue;

                                    int amount = Random.Range(drop.minAmount, drop.maxAmount);

                                    for (int i = 0; i < amount; i++)
                                    {
                                        GameObject dropGO = Instantiate(dropPrefab, spawnPosition, Quaternion.identity);
                                        DropComponent dropComp = dropGO.GetComponent<DropComponent>();
                                        dropComp.player = transform;
                                        dropComp.SetItem(drop.item);
                                        Destroy(dropGO, 300f);
                                    }
                                }

                                WorldData.World.SetPropType(checkX, checkY, PropType.None);

                                int chunkX = checkX / Chunk.ChunkSize;
                                int chunkY = checkY / Chunk.ChunkSize;
                                WorldManager.Instance.chunks[chunkX, chunkY].UpdateTile(checkX, checkY);

                                foundProp = true;
                                break;
                            }
                        }

                        if (foundProp) break;
                    }
                }

                // TODO: ATTACK LOGIC FOR ENEMIES
            }
        }
    }
}
