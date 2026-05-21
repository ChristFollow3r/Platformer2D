using Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlaceFeedback : MonoBehaviour
    {
        #region Data
        private const float CellSize = 0.5f;
        private Vector2 currentCell;
        private LineRenderer lineRenderer;
        private BreakAndPlace breakAndPlace;
        #endregion

        #region Unity
        private void Awake()
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            breakAndPlace = GetComponent<BreakAndPlace>();
            lineRenderer.loop = true;
            lineRenderer.positionCount = 4;
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingLayerName = "Default";
            lineRenderer.sortingOrder = 10;

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = Color.gray;
            lineRenderer.endColor = Color.gray;

            lineRenderer.enabled = false;
        }

        private void Update()
        {
            ItemStack handItem = Inventory.Singleton.hand;
            if (handItem == null || !handItem.data.isPlacable)
            {
                Undraw();
                return;
            }

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 cell = new Vector2(Mathf.FloorToInt(mousePos.x / CellSize), Mathf.FloorToInt(mousePos.y / CellSize));
            Vector2 cellWorldPos = cell * CellSize + Vector2.one * (CellSize / 2f);
            float distance = Vector2.Distance(cellWorldPos, transform.position);
            if (distance < 0.5f)
            {
                Undraw();
                return;
            }
            if (distance >= breakAndPlace.reachDistance)
            {
                Undraw();
                return;
            }

            if (cell != currentCell)
            {
                currentCell = cell;
                Draw();
            }

            lineRenderer.enabled = true;
        }
        #endregion

        #region Methods
        private void Undraw()
        {
            lineRenderer.enabled = false;
        }
        private void Draw()
        {
            // Bottom-left corner of the cell in world space
            Vector3 origin = new Vector3(currentCell.x * CellSize, currentCell.y * CellSize, 0f);

            lineRenderer.SetPosition(0, origin);
            lineRenderer.SetPosition(1, origin + new Vector3(CellSize, 0f, 0f));
            lineRenderer.SetPosition(2, origin + new Vector3(CellSize, CellSize, 0f));
            lineRenderer.SetPosition(3, origin + new Vector3(0f, CellSize, 0f));
        }
        #endregion
    }
}
