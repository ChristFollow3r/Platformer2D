using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Inventory : MonoBehaviour
    { 
        
        [Header("UI")]
        [SerializeField] private GameObject hotbar;
        [SerializeField] private GameObject inventory;
        [SerializeField] private GameObject craftingMenu;
        [SerializeField] private Player.PlayerManager playerManager;

        bool inventoryOpened = false;
        private void Start()
        {
            var hotBarData = playerManager.Inventory.GetHotBarSlots();
            
            for (int i = 0; i < 9; i++)
                hotbar.transform.GetChild(i).GetComponent<SlotUI>().SetSlot(hotBarData[i]);
            
            var inventoryData = playerManager.Inventory.GetInventorySlots();
            
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    inventory.transform.GetChild(i * 9 + j).GetComponent<SlotUI>().SetSlot(inventoryData[i, j]);
                }
            }

            var craftingData = playerManager.Inventory.GetCraftingSlots();
            var resultData = playerManager.Inventory.GetResultSlot();
            
            for (int i = 0; i < 16; i++)
            {
                var slotUI = craftingMenu.transform.GetChild(i).GetComponent<SlotUI>();
                if (slotUI != null)
                    slotUI.SetSlot(craftingData[i]);
            }
            
            var resultUI = craftingMenu.transform.GetChild(16).GetComponent<SlotUI>();
            if (resultUI != null)
                resultUI.SetSlot(resultData);
            
            Data.Inventory.InventoryManager.Instance.SetCraftingSlots(craftingData, resultData); // This I had to ask AI. I've been here for 4 hours debugging this shit just to realize I was using different fucking arrays
            inventory.SetActive(false);
            craftingMenu.SetActive(false);
        }

        private void Update()
        {
            if (playerManager.PlayerMovement.playerInput.Player.InventoryToggle.WasPerformedThisFrame())
            {
                inventoryOpened = !inventoryOpened;
                inventory.SetActive(inventoryOpened);
                craftingMenu.SetActive(inventoryOpened);
            }
            
        }
    }
}
