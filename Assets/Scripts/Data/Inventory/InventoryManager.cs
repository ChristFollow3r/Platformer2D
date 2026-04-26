using Scriptable_Objects_Scripts;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using Player;
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
        
        private InventorySlot[] craftingSlots;
        private InventorySlot resultSlot;
        private Color ghostIconColor;

        private Inventory playerInvetory;
        private int heldItemIndex = 0;
        
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
            playerInvetory = player.GetComponent<Player.PlayerManager>().Inventory;
            ghostIconColor = ghostIcon.color;
            MouseGhostVisuals();
        }
        
        private void Update()
        {
            GetKeyboardIndex();
            
            if (mouseSlot.IsEmpty) return;
            mouseGhost.transform.position = Input.mousePosition;
        }
        
        public void SetCraftingSlots(InventorySlot[] slots, InventorySlot result)
        {
            craftingSlots = slots;
            resultSlot = result;
            Debug.Log("Crafting slots assigned to Manager!");
        }

        #region Mouse Left Click
        public void HandleLeftClick(InventorySlot clickedSlot)
        {
            if (clickedSlot.IsEmpty && mouseSlot.IsEmpty) return;
            // Result slot shit
            if (clickedSlot == resultSlot)
            {
                if (resultSlot.IsEmpty || !mouseSlot.IsEmpty) return;
                
                mouseSlot.AddItem(clickedSlot.GetItem(), clickedSlot.GetAmount());
                resultSlot.Remove(resultSlot.GetAmount());

                foreach (var s in craftingSlots) s.Remove(1);
                
                UpdateCraftingOutput();
                MouseGhostVisuals();
                return;
            }
            // Holding nothing - Pick the entire stack - WORKS
            if (mouseSlot.IsEmpty)
            {
                mouseSlot.AddItem(clickedSlot.GetItem(), clickedSlot.GetAmount());
                clickedSlot.Remove(clickedSlot.GetAmount());
            }
            else if (!mouseSlot.IsEmpty)
            {
                // Holding something + clicking - drop the entire stack - WORKS
                if (clickedSlot.IsEmpty)
                {
                    clickedSlot.AddItem(mouseSlot.GetItem(), mouseSlot.GetAmount());
                    mouseSlot.Remove(mouseSlot.GetAmount());
                }
                // Holding same item - Fill stack until full - WORKS
                else if (clickedSlot.CanBeStacked(mouseSlot.GetItem()))
                {
                    int startingAmount = mouseSlot.GetAmount();
                    int remainingAmount = clickedSlot.AddItem(mouseSlot.GetItem(), startingAmount);
                    mouseSlot.Remove(startingAmount - remainingAmount);
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
                }
            }
            if (IsCraftingSLot(clickedSlot))
                UpdateCraftingOutput();
            MouseGhostVisuals();
        }
        #endregion
        
        #region Mouse Right Click
        public void HandleRightClick(InventorySlot clickedSlot)
        {
            if (clickedSlot.IsEmpty && mouseSlot.IsEmpty) return;
            // Holding nothing - pick half the stack
            if (mouseSlot.IsEmpty)
            {
                mouseSlot.AddItem(clickedSlot.GetItem(), clickedSlot.GetAmount() / 2);
                clickedSlot.Remove(clickedSlot.GetAmount() / 2);
            }
            // Holding something - Drope one
            else if (!mouseSlot.IsEmpty)
            {
                clickedSlot.AddItem(mouseSlot.GetItem(), 1);
                mouseSlot.Remove(1);
            }
            if (IsCraftingSLot(clickedSlot))
                UpdateCraftingOutput();
            MouseGhostVisuals();
        }
        #endregion

        private void UpdateCraftingOutput() // Not finished
        {
            if (resultSlot is null)
            {
                Debug.Log("ResultSlot is null");
                return;
            }
            resultSlot.Remove(resultSlot.GetAmount());
            
            foreach (Recipe recipe in recipes)
            {
                Debug.Log("UpdateCraftingOutput does something");
                if (Matches(recipe))
                {
                    Debug.Log("Recipe matches!");
                    resultSlot.AddItem(recipe.result, 1);
                    return;
                }
            }
        }

        private bool Matches(Recipe recipe)
        {
            Debug.Log("I AM WORKING");
            for (int i = 0; i < 16; i++)
            {
                Item slotItem = craftingSlots[i].GetItem();
                Item recipeItem = recipe.ingredients[i];
                if (slotItem != recipeItem) 
                    return false;
            }
            return true;
        }

        private bool IsCraftingSLot(InventorySlot slot)
        {
            Debug.Log($"Checking slot. CraftingSlots size: {(craftingSlots?.Length ?? -1)}");
            if (slot == resultSlot) return true;
            if (craftingSlots is null)  return false;
            
            foreach (var s in craftingSlots)
            {
                if (s == slot) return true;
            }

            return false;
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
                ghostIconColor.a = 0f;

            else
            {
                ghostIconColor.a = 1f;
                ghostIcon.sprite = mouseSlot.GetItem().itemIcon;
                ghostText.text = mouseSlot.GetAmount() <= 1 ? "" : mouseSlot.GetAmount().ToString();
            }
            
            ghostIcon.color = ghostIconColor;
        }

        public Item GetHeldItem()
        {
            var hotbar = playerInvetory.GetHotBarSlots();
            return hotbar[heldItemIndex].GetItem();
        }

        private void GetKeyboardIndex()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) heldItemIndex = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) heldItemIndex = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) heldItemIndex = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) heldItemIndex = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) heldItemIndex = 4;
            else if (Input.GetKeyDown(KeyCode.Alpha6)) heldItemIndex = 5;
            else if (Input.GetKeyDown(KeyCode.Alpha7)) heldItemIndex = 6;
            else if (Input.GetKeyDown(KeyCode.Alpha8)) heldItemIndex = 7;
            else if (Input.GetKeyDown(KeyCode.Alpha9)) heldItemIndex = 8;
        }
    }
}
