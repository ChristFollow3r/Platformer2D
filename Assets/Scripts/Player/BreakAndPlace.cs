using System.Collections.Generic;
using Chunks;
using Data;
using UnityEngine;
using UnityEngine.InputSystem;
using Scriptable_Objects_Scripts;

namespace Player
{
    public class BreakAndPlace : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        [SerializeField] private ScriptableObject placeHolder;
        [SerializeField] private int reachDistance;
        [SerializeField] private List<Prop> allProps;
        
        private float breakTimer = 0f;
        private Vector2Int lastMousePosition;
        
        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            BuildingAndBreaking();
        }
        private void BuildingAndBreaking()
        {
            Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            float distance = Vector2.Distance(mousePosition, player.transform.position);
            
            int mouseX = Mathf.FloorToInt(mousePosition.x);
            int mouseY = Mathf.FloorToInt(mousePosition.y);


            if (Mathf.Abs(distance) > reachDistance)
            {
                breakTimer = 0f;
                return;
            }
            
            if (Mouse.current.leftButton.isPressed)
            {
                if (lastMousePosition.x !=  mouseX || lastMousePosition.y != mouseY)
                {
                    breakTimer = 0f;
                    lastMousePosition = new Vector2Int(mouseX, mouseY);
                }
                
                PropType clickedType = WorldData.World.GetPropType(mouseX, mouseY);
                BlockType clickedBlock = WorldData.World.GetBlockTypes(mouseX, mouseY);

                float targetHardness = 0f;
                bool isBreakingABlock = false;

                if (clickedType != PropType.None)
                {
                    targetHardness = WorldData.PropDictionary[clickedType].hardness;
                    isBreakingABlock = false;
                }
                
                else if (clickedBlock != BlockType.Air)
                {
                    targetHardness = WorldData.BlockDictionary[clickedBlock].hardness;
                    isBreakingABlock = true;
                }
                
                else return;
                
                var heldItem = Data.Inventory.InventoryManager.Instance.GetHeldItem();
                float itemStrength = heldItem?.tier ?? 0.5f; // Fucking rider is the goat fixing my shitty code
                
                breakTimer += Time.deltaTime * itemStrength;
                if (breakTimer >= targetHardness)
                {
                    if (isBreakingABlock)
                    {
                        var blockData = WorldData.BlockDictionary[clickedBlock];
                        var drop = Instantiate(WorldData.BlockDictionary[clickedBlock].blockPrefab, new Vector2(mouseX, mouseY), Quaternion.identity);
                        Destroy(drop, 300f);
                        WorldData.World.SetBlockType(mouseX, mouseY, BlockType.Air);
                    }
                    
                    else
                    {
                        // DROP LOGIC FOR PROPS (Your existing code)
                        Prop propData = WorldData.PropDictionary[clickedType];
                        foreach (Drop drop in propData.drops)
                        {
                            if (Random.Range(0, 101) <= drop.dropChance)
                            {
                                for (int i = 0; i < drop.amount; i++)
                                {
                                    var droppedItem = Instantiate(drop.item.drop, new Vector2(mouseX, mouseY), Quaternion.identity);
                                    if (droppedItem is not null) Destroy(droppedItem, 300f);
                                }
                            }
                        }
                        WorldData.World.SetPropType(mouseX, mouseY, PropType.None);
                    }

                    // Update the visual tile
                    WorldManager.Instance.chunks[(mouseX / Chunk.ChunkSize), (mouseY / Chunk.ChunkSize)].UpdateTile(mouseX, mouseY);
                    breakTimer = 0f;
                }
            }

            else
            {
                breakTimer = 0f;
            }

            if (Mouse.current.rightButton.isPressed) // This will be changed eventually taking into an account what the player is holding
            {
                if (lastMousePosition.x == mouseX && lastMousePosition.y == mouseY) return;
                lastMousePosition = new Vector2Int(mouseX, mouseY);
                
                var heldItem = Data.Inventory.InventoryManager.Instance.GetHeldItem();
                if (heldItem is null) return;
                
                if (heldItem.blockType != BlockType.None && heldItem.blockType != BlockType.Air)
                {
                    if (WorldData.World.GetBlockTypes(mouseX, mouseY) == BlockType.Air && WorldData.World.GetPropType(mouseX, mouseY) == PropType.None)
                    {
                        WorldData.World.SetBlockType(mouseX, mouseY, heldItem.blockType);
                        WorldManager.Instance.chunks[(mouseX / Chunk.ChunkSize), (mouseY / Chunk.ChunkSize)].UpdateTile(mouseX, mouseY);
                        Data.Inventory.InventoryManager.Instance.UseBlock();
                    }
                }
                
            }
            
        }

    }
}
