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

        private void Update()
        {
            BuildingAndBreaking();
        }
        private void BuildingAndBreaking()
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            float distance = Vector2.Distance(mousePosition, player.transform.position);
            int mouseX = Mathf.FloorToInt(mousePosition.x);
            int mouseY = Mathf.FloorToInt(mousePosition.y);


            if (Mathf.Abs(distance) > reachDistance) return;

            if (Mouse.current.leftButton.isPressed)
            {
                WorldData.World.SetBlockType(mouseX, mouseY, BlockType.Air);
                WorldManager.Instance.chunks[(mouseX / 16), (mouseY / 16)].UpdateTile(mouseX, mouseY);
            }

            if (Mouse.current.rightButton.isPressed)
            {
                WorldData.World.SetBlockType(mouseX, mouseY, BlockType.Grass);
                WorldManager.Instance.chunks[(mouseX / 16), (mouseY / 16)].UpdateTile(mouseX, mouseY);
            }
        }

    }
}
