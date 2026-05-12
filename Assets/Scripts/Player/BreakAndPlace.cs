using System.Collections.Generic;
using Chunks;
using Data;
using UnityEngine;
using UnityEngine.InputSystem;
using Scriptable_Objects_Scripts;
using World;

namespace Player
{
    public class BreakAndPlace : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        [SerializeField] private ScriptableObject placeHolder; // Shit
        [SerializeField] private List<Prop> allProps;

        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private int reachDistance;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private Vector2 attackBoxSize = new Vector2(1.5f, 1.5f);

        [Header("Mining Data")]
        private Vector2Int currentMineTarget = new Vector2Int(-999, -999);
        private float currentBlockDamage = 0f;

        private Camera mainCamera;
        private Collider2D playerCollider;

        private void OnEnable()
        {
            if (playerMovement is not null)
            {
                playerMovement.OnAttackPerformed += HandleAttacking;
                playerMovement.OnMinePerformed += HandleMining;
            }
        }

        private void OnDisable()
        {
            if (playerMovement != null)
            {
                playerMovement.OnAttackPerformed -= HandleAttacking;
                playerMovement.OnMinePerformed -= HandleMining;
            }
        }

        private void Awake()
        {
            mainCamera = Camera.main;

            if (player != null)
            {
                playerCollider = player.GetComponent<Collider2D>();
            }
        }

        private void HandleAttacking(Vector2 mouseWorldPosition)
        {
            StartCoroutine(DelayedAttackRoutine(mouseWorldPosition));
        }

        private void HandleMining(Vector2 mouseWorldPosition)
        {
            Vector2 playerCenter = playerCollider.bounds.center;
            float distance = Vector2.Distance(mouseWorldPosition, playerCenter);

            if (distance > reachDistance) return;

            float cellSize = 0.5f;
            int mouseX = Mathf.FloorToInt(mouseWorldPosition.x / cellSize);
            int mouseY = Mathf.FloorToInt(mouseWorldPosition.y / cellSize);

            if (currentMineTarget.x != mouseX || currentMineTarget.y != mouseY)
            {
                currentMineTarget = new Vector2Int(mouseX, mouseY);
                currentBlockDamage = 0f;
            }

            BlockType clickedBlock = WorldData.World.GetBlockTypes(mouseX, mouseY);

            if (clickedBlock != BlockType.Air)
            {
                float targetHardness = WorldData.BlockDictionary[clickedBlock].hardness;
                float miningPower = 1f; // TODO: Base this on the player upgrade

                currentBlockDamage += miningPower;

                if (currentBlockDamage >= targetHardness)
                {
                    // TODO: break block, drop shit, add sound, add particles
                    BreakBlock(mouseX, mouseY, clickedBlock);
                    currentBlockDamage = 0f;
                    Debug.Log("Block broken");
                }
            }
        }

        private void BreakBlock(int x, int y, BlockType type)
        {
            float cellSize = 0.5f;
            Vector2 spawnPosition = new Vector2(x * cellSize, y * cellSize);

            // SPAWNING BLOCKS IN HERE.
            var drop = Instantiate(WorldData.BlockDictionary[type].blockPrefab, spawnPosition, Quaternion.identity);
            Destroy(drop, 300f);

            WorldData.World.SetBlockType(x, y, BlockType.Air);
            int chunkX = x / Chunk.ChunkSize;
            int chunkY = y / Chunk.ChunkSize;
            WorldManager.Instance.chunks[chunkX, chunkY].UpdateTile(x, y);
        }

        private System.Collections.IEnumerator DelayedAttackRoutine(Vector2 mouseWorldPosition)
        {
            yield return new WaitForSeconds(0.30f);

            Vector2 playerCenter = playerCollider.bounds.center;
            Vector2 attackDirection = (mouseWorldPosition - playerCenter).normalized;
            Vector2 attackCenter = playerCenter + (attackDirection * attackRange);

            Collider2D[] hitObjects = Physics2D.OverlapBoxAll(attackCenter, attackBoxSize, 0f);

            #region HitBoxDebug
            Vector2 min = attackCenter - (attackBoxSize / 2f);
            Vector2 max = attackCenter + (attackBoxSize / 2f);
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
                                // TODO: Instantiate drop

                                WorldData.World.SetPropType(checkX, checkY, PropType.None);

                                int chunkX = checkX / Chunk.ChunkSize;
                                int chunkY = checkY / Chunk.ChunkSize;
                                WorldManager.Instance.chunks[chunkX, chunkY].UpdateTile(checkX, checkY);

                                Debug.Log($"{propHitData.name} hit at anchor [{checkX}, {checkY}]"); // TODO: Remove
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
