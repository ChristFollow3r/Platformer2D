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


            if (Mathf.Abs(distance) > reachDistance) return;
            if (Mouse.current.leftButton.isPressed)
            {
                PropType clickedType = WorldData.World.GetPropType(mouseX, mouseY);
                if (clickedType == PropType.None) return;
                Prop propData = null;
                foreach (Prop prop in allProps)
                {
                    if (prop.type == clickedType)
                    {
                        propData = prop;
                        break;
                    }
                }

                if (propData is not null)
                {
                    foreach (Drop drop in propData.drops)
                    {
                        if (Random.Range(0, 101) <= drop.dropChance)
                        {
                            for (int i = 0; i < drop.amount; i++)
                            {
                                var droppedItem = Instantiate(drop.item.drop, new Vector2(mouseX, mouseY), Quaternion.identity); // Best naming ever lol
                                if (droppedItem is not null) Destroy(droppedItem, 300f);
                            }
                        }
                    }
                }
            }

            if (Mouse.current.rightButton.isPressed) // This will be changed eventually taking into an account what the player is holding
            {
                WorldData.World.SetBlockType(mouseX, mouseY, BlockType.Grass);
                WorldManager.Instance.chunks[(mouseX / Chunk.ChunkSize), (mouseY / Chunk.ChunkSize)].UpdateTile(mouseX, mouseY);
            }
            
        }

    }
}
