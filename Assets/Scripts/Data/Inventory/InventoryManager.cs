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
        [SerializeField] private InventorySlot[] craftingSlots;
        [SerializeField] private InventorySlot resultSlot;
        
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

        #region Mouse Left Click
        public void HandleLeftClick(InventorySlot clickedSlot)
        {
            // Holding nothing - Pick the entire stack - WORKS
            if (mouseSlot.IsEmpty)
            {
                mouseSlot.AddItem(clickedSlot.GetItem(), clickedSlot.GetAmount());
                clickedSlot.Remove(clickedSlot.GetAmount());
                MouseGhostVisuals();
            }
            else if (!mouseSlot.IsEmpty)
            {
                // Holding something + clicking - drop the entire stack - WORKS
                if (clickedSlot.IsEmpty)
                {
                    clickedSlot.AddItem(mouseSlot.GetItem(), mouseSlot.GetAmount());
                    mouseSlot.Remove(mouseSlot.GetAmount());
                    MouseGhostVisuals();
                }
                // Holding same item - Fill stack until full - WORKS
                else if (clickedSlot.CanBeStacked(mouseSlot.GetItem()))
                {
                    int startingAmount = mouseSlot.GetAmount();
                    int remainingAmount = clickedSlot.AddItem(mouseSlot.GetItem(), startingAmount);
                    mouseSlot.Remove(startingAmount - remainingAmount);
                    MouseGhostVisuals();
                }
                // Holding different item - swap entire stack - WORKS
                else if (!clickedSlot.CanBeStacked(mouseSlot.GetItem()))
                {
                    var mouseSlotItem = mouseSlot.GetItem();
                    var clickedSlotItem = clickedSlot.GetItem();
                    
                    int mouseSlotAmount = mouseSlot.GetAmount();
                    int clickedSlotAmount =  clickedSlot.GetAmount();
                    
                    mouseSlot.Remove(mouseSlotAmount);
                    clickedSlot.Remove(clickedSlotAmount);
                    
                    mouseSlot.AddItem(clickedSlotItem, clickedSlotAmount);
                    clickedSlot.AddItem(mouseSlotItem ,mouseSlotAmount);
                    MouseGhostVisuals();
                }
            }
        }
        #endregion
        #region Mouse Right Click
        public void HandleRightClick(InventorySlot clickedSlot)
        {
            // Holding nothing - pick half the stack
            if (mouseSlot.IsEmpty)
            {
                mouseSlot.AddItem(clickedSlot.GetItem(), clickedSlot.GetAmount() / 2);
                clickedSlot.Remove(clickedSlot.GetAmount() / 2);
                MouseGhostVisuals();
            }
            // Holding something - Drope one
            else if (!mouseSlot.IsEmpty)
            {
                clickedSlot.AddItem(mouseSlot.GetItem(), 1);
                mouseSlot.Remove(1);
                MouseGhostVisuals();
            }
        }
        #endregion

        public void SetCraftingSlots(InventorySlot[] slots, InventorySlot output)
        {
            craftingSlots = slots;
            resultSlot = output;
        }

        public void UpdateCraftingOutput() // Not finishedd
        {
            if (resultSlot is null) return;
            foreach (Recipe recipe in recipes)
            {
                if (Matches(recipe))
                {
                    Debug.Log("Recipe matches!");
                }
            }
        }

        private bool Matches(Recipe recipe)
        {
            for (int i = 0; i < 16; i++)
            {
                if (craftingSlots[i].GetItem() != recipe.ingredients[i]) 
                    return false;
            }
            return true;
        }

        public void DropItem()
        {
            if (!mouseSlot.IsEmpty) // My first approach was working but wrong cause unity UI is trash, so I asked AI to fix it and called it a day.
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
