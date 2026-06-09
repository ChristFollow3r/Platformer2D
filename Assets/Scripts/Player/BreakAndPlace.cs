using System;
using System.Collections.Generic;
using Chunks;
using Data;
using UnityEngine;
using Scriptable_Objects_Scripts;
using World;
using Items;
using Items.Overlays;
using System.Linq;

namespace Player
{
    public class BreakAndPlace : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PlayerMovement playerMovement;

        [Header("Controls")]
        public int reachDistance = 1;

        [Header("Prop Attack Hitbox")]
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

        [Header("Feedback")]
        [SerializeField] private Material whiteFlashMaterial;

        private Vector2 cachedTargetPosition;
        private readonly Collider2D[] hitResults = new Collider2D[10];

        public event Action OnPlacePerformed;
        public event Action OnBlockBroken;

        private BlockType[] entities = new BlockType[]
        {
            BlockType.Chest,
            BlockType.CraftingTable,
            BlockType.Furnace
        };

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
            if (TutorialUI.IsInputBlocked) return;

            Vector3 realScreenPos = Input.mousePosition;
            float camZ = Mathf.Abs(Camera.main.transform.position.z);
            Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(realScreenPos.x, realScreenPos.y, camZ));

            if (Input.GetMouseButtonDown(1))
            {
                BlockType clickedBlock = GetClickedBlock(worldMousePos, out ulong blockId);

                if (IsBlockEntity(clickedBlock))
                {
                    UIController.Singleton.OpenOverlay(blockId);
                    return;
                }

                if (clickedBlock == BlockType.spawnAnchor)
                {
                    bool spawnUpdated = WorldManager.Instance.TrySetSpawnFromAnchor(worldMousePos);

                    ItemData anchorData = WorldData.BlockDictionary[clickedBlock];

                    if (anchorData != null && anchorData.sprite != null)
                    {
                        int x = Mathf.FloorToInt(worldMousePos.x / CellSize);
                        int y = Mathf.FloorToInt(worldMousePos.y / CellSize);
                        Vector2 cellCenter = new Vector2(x, y) * CellSize + new Vector2(0.25f, 0.25f);

                        StartCoroutine(HitFlashRoutine(anchorData.sprite, cellCenter));
                    }

                    if (spawnUpdated)
                    {
                        Debug.Log($"Spawn anchor activated! New spawn point: {WorldManager.Instance.currentSpawnPoint}");
                    }
                    return;
                }
            }

            if (Input.GetMouseButton(1))
            {
                ItemStack item = Inventory.Singleton.hand;

                if (item != null && item.data.isPlacable)
                {
                    TryPlaceBlock(item, worldMousePos);
                }
            }
        }

        private void OnAttackInputReceived(Vector2 ignoredMousePosition)
        {
            Vector3 realScreenPos = Input.mousePosition;
            float camZ = Mathf.Abs(Camera.main.transform.position.z);
            Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(realScreenPos.x, realScreenPos.y, camZ));

            cachedTargetPosition = worldMousePos;
        }

        private void OnMineInputReceived(Vector2 ignoredMousePosition)
        {
            Vector3 realScreenPos = Input.mousePosition;
            float camZ = Mathf.Abs(Camera.main.transform.position.z);
            Vector2 worldMousePos = Camera.main.ScreenToWorldPoint(new Vector3(realScreenPos.x, realScreenPos.y, camZ));

            cachedTargetPosition = worldMousePos;
        }

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
                                float hitPower = Equipment.Singleton.GetHitPower();
                                currentPropDamage += hitPower;

                                if (propHitData.hitSound != null)
                                {
                                    AudioSource.PlayClipAtPoint(propHitData.hitSound, Camera.main.transform.position);
                                }

                                if (propHitData.sprite != null)
                                {
                                    Vector2 cellCenter = new Vector2(checkX, checkY) * CellSize + new Vector2(0.25f, 0.25f);
                                    StartCoroutine(HitFlashRoutine(propHitData.sprite, cellCenter));
                                }

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

            if (distance > reachDistance) return;

            int x = Mathf.FloorToInt(actualMouseWorldPos.x / CellSize);
            int y = Mathf.FloorToInt(actualMouseWorldPos.y / CellSize);

            if (WorldData.World.SafeCheck(x, y + 1))
            {
                PropType propAbove = WorldData.World.GetPropType(x, y + 1);
                if (propAbove != PropType.None)
                {
                    currentBlockDamage = 0f;
                    return;
                }
            }

            if (currentMineTarget.x != x || currentMineTarget.y != y)
            {
                currentMineTarget = new Vector2Int(x, y);
                currentBlockDamage = 0f;
            }

            BlockType clickedBlock = WorldData.World.GetBlockTypes(x, y);

            if (clickedBlock == BlockType.Air) return;

            ItemData blockHitData = WorldData.BlockDictionary[clickedBlock];

            float targetHardness = blockHitData.hardness;
            float miningPower = Equipment.Singleton.GetMiningPower();

            currentBlockDamage += miningPower;

            if (blockHitData.hitSound != null)
            {
                AudioSource.PlayClipAtPoint(blockHitData.hitSound, Camera.main.transform.position);
            }

            if (blockHitData.sprite != null)
            {
                Vector2 cellCenter = new Vector2(x, y) * CellSize + new Vector2(0.25f, 0.25f);
                StartCoroutine(HitFlashRoutine(blockHitData.sprite, cellCenter));
            }

            if (currentBlockDamage >= targetHardness)
            {
                BreakBlock(x, y, clickedBlock);
                currentBlockDamage = 0f;

                if (IsBlockEntity(clickedBlock))
                {
                    UIController.Singleton.DestroyEntity(BlockIdUtils.From(x, y));
                }
            }
        }

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

            if (distance > 2f) return true;

            BlockType clickedBlock = WorldData.World.GetBlockTypes(x, y);
            if (clickedBlock != BlockType.Air) return true;

            if (distance < 0.5f) return true;

            WorldData.World.SetBlockType(x, y, item.data.blockType);
            UpdateChunkVisuals(x, y);
            WorldManager.Instance.UpdateDynamicLighting(x, y);

            if (item.data.hitSound != null)
            {
                AudioSource.PlayClipAtPoint(item.data.hitSound, Camera.main.transform.position);
            }

            Inventory.Singleton.RemoveFromHand();

            if (IsBlockEntity(item.data.blockType))
            {
                UIController.Singleton.CreateOverlay(BlockIdUtils.From(x, y), item.data.overlayType);
            }

            OnPlacePerformed?.Invoke();

            return false;
        }

        private void BreakBlock(int x, int y, BlockType type)
        {
            Vector2 spawnPosition = new Vector2(x * CellSize + 0.25f, y * CellSize + 0.25f);

            ItemData blockData = WorldData.BlockDictionary[type];

            if (blockData.drops != null && blockData.drops.Count > 0)
            {
                foreach (Drop drop in blockData.drops)
                {
                    bool doesDrop = UnityEngine.Random.Range(0, 100) <= drop.dropChance;
                    if (!doesDrop) continue;

                    int amount = UnityEngine.Random.Range(drop.minAmount, drop.maxAmount + 1);

                    for (int i = 0; i < amount; i++)
                    {
                        SpawnLoot(drop.item, spawnPosition);
                    }
                }
            }
            else
            {
                SpawnLoot(blockData, spawnPosition);
            }

            WorldData.World.SetBlockType(x, y, BlockType.Air);
            UpdateChunkVisuals(x, y);
            WorldManager.Instance.UpdateDynamicLighting(x, y);

            OnBlockBroken?.Invoke();
        }

        private void BreakProp(PropType hitType, int checkX, int checkY)
        {
            Prop propHitData = WorldData.PropDictionary[hitType];
            Vector2 spawnPosition = new Vector2(checkX, checkY) * CellSize + new Vector2(0.25f, 0.25f);

            foreach (Drop drop in propHitData.drops)
            {
                bool doesDrop = UnityEngine.Random.Range(0, 100) <= drop.dropChance;
                if (!doesDrop) continue;

                int amount = UnityEngine.Random.Range(drop.minAmount, drop.maxAmount + 1);

                for (int i = 0; i < amount; i++)
                {
                    SpawnLoot(drop.item, spawnPosition);
                }
            }

            WorldData.World.SetPropType(checkX, checkY, PropType.None);
            UpdateChunkVisuals(checkX, checkY);
            WorldManager.Instance.UpdateDynamicLighting(checkX, checkY);

            OnBlockBroken?.Invoke();
        }

        private void SpawnLoot(ItemData itemData, Vector2 position)
        {
            if (itemData == null || itemEntityPrefab == null) return;

            GameObject droppedItem = Instantiate(itemEntityPrefab, position, Quaternion.identity);
            Vector2 randomOffset = new Vector2(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(-0.2f, 0.2f));
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

        private bool IsBlockEntity(BlockType blockType) => entities.Contains(blockType);

        private System.Collections.IEnumerator HitFlashRoutine(Sprite propSprite, Vector2 position)
        {
            GameObject flashObj = new GameObject("PropHitFlash");
            flashObj.transform.position = position;

            SpriteRenderer sr = flashObj.AddComponent<SpriteRenderer>();
            sr.sprite = propSprite;
            sr.sortingOrder = 999;

            if (whiteFlashMaterial != null)
            {
                sr.material = whiteFlashMaterial;
            }

            yield return new WaitForSeconds(0.1f);
            Destroy(flashObj);
        }
    }
}
