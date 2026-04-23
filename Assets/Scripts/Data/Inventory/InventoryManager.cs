using Scriptable_Objects_Scripts;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Image = UnityEngine.UI.Image;


namespace Data.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private GameObject mouseGhost;
        [SerializeField] private Image ghostIcon;
        [SerializeField] private TextMeshProUGUI ghostText;
        [SerializeField] private GameObject player;

        [SerializeField] private List<Recipe> recipes;
        private static InventoryManager _instance;
        public static InventoryManager Instance => _instance;
        private readonly InventorySlot mouseSlot = new InventorySlot();
        private void Awake()
        {
            if (_instance == null) _instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            MouseGhostVisuals();
        }
        
        private void Update()
        {
            if (!mouseGhost.activeSelf) return;
            mouseGhost.transform.position = Input.mousePosition;
        }
        

        public void HandleLeftClick(InventorySlot clickedSlot)
        {
            // Rewrite this shit
        }

        public void HandleRightClick(InventorySlot clickedSlot)
        {
            // Rewrite this shit
        }

        public void UpdateCraftingResults()
        {
            
        }

        public void DropItem()
        {
            if (!mouseSlot.IsEmpty) // My first aproach was working but wrong cause unity UI is trash, so I asked AI to fix it and called it a day.
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition); // If the camera is Null GG
                Vector3 spawnPos = Physics.Raycast(ray, out RaycastHit hit) ? hit.point + Vector3.up * 0.5f : ray.GetPoint(5f); // Copy-paste from AI
                // I don't want to think this trough. Fuck unity and its stupid 2D games that are 3D
                Instantiate(mouseSlot.GetItem().drop, spawnPos, Quaternion.identity);
                mouseSlot.Remove(1);
                MouseGhostVisuals();
            }
        }

        private void MouseGhostVisuals()
        {
            if (mouseSlot.IsEmpty)
                mouseGhost.SetActive(false);

            else
            {
                mouseGhost.SetActive(true);
                ghostIcon.sprite = mouseSlot.GetItem().itemIcon;
                ghostText.text = mouseSlot.GetAmount() <= 1 ? "" : mouseSlot.GetAmount().ToString();
            }
        }
    }
}
