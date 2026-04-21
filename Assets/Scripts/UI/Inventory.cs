using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Inventory : MonoBehaviour
    {
        public static Inventory Instance;
        [SerializeField] private GameObject hotbar;
        [SerializeField] private GameObject inventory;
        [SerializeField] private GameObject craftingMenu;
        [SerializeField] private Player.PlayerManager playerManager;

        bool inventoryOpened = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
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
            for (int i = 0; i < 17; i++)
                craftingMenu.transform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = false; // This will be changed
            
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
