using System;
using Chunks;
using Data;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class BreakAndPlace : MonoBehaviour
    {
        [SerializeField] private GameObject player;
        [SerializeField] private ScriptableObject placeHolder;
        [SerializeField] private int reachDistance;
        [SerializeField] private GameObject[] drops;
        
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
                if (WorldData.World.GetPropType(mouseX, mouseY) == PropType.Bush)
                {
                    Instantiate(drops[0], new Vector2(mouseX , mouseY), Quaternion.identity);
                    WorldData.World.SetPropType(mouseX, mouseY, PropType.None);
                    WorldManager.Instance.chunks[(mouseX / Chunks.Chunk.ChunkSize), (mouseY / Chunks.Chunk.ChunkSize)].UpdateTile(mouseX, mouseY);
                }

                return;
            }

            if (Mouse.current.rightButton.isPressed)
            {
                WorldData.World.SetBlockType(mouseX, mouseY, BlockType.Grass);
                WorldManager.Instance.chunks[(mouseX / Chunks.Chunk.ChunkSize), (mouseY / Chunks.Chunk.ChunkSize)].UpdateTile(mouseX, mouseY);
            }
        }

    }
}
