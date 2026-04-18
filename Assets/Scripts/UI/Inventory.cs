using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private GameObject hotbar;
        [SerializeField] private GameObject inventory;
        [SerializeField] private Player.PlayerManager playerManager;
        
        bool inventoryOpened = false;

        private void Awake()
        {
            foreach (var slot in playerManager.Inventory.GetHotBarSlots())
                slot.OnSlotChanged += UpdateHotbarUI;
        }

        private void Start()
        {
            for (int i =  0; i < 9; i++)
                hotbar.transform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = false;
        }

        private void Update()
        {
            OpenInventoryUI();
        }

        private void UpdateHotbarUI()
        {
            var hotBarSlots = playerManager.Inventory.GetHotBarSlots();
            Debug.Log("I'm being called");
            for (int i = 0; i < 9; i++)
            {
                Debug.Log("Hotbar Slot: " + i +  "being filled");
                if (hotBarSlots[i].IsEmpty)
                {
                    hotbar.transform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = false;
                    continue;
                }
                
                hotbar.transform.GetChild(i).GetChild(0).GetComponent<Image>().enabled = true;
                Sprite itemSprite = hotBarSlots[i].GetItem().itemIcon;
                hotbar.transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = itemSprite;
                hotbar.transform.GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text =
                    hotBarSlots[i].GetAmount().ToString();
            }
        }

        private void OpenInventoryUI()
        {
            if (playerManager.PlayerMovement.playerInput.Player.InventoryToggle.WasPerformedThisFrame()) // Maybe disable player movement while in the inventory?
            {
                inventoryOpened = !inventoryOpened;
                inventory.SetActive(inventoryOpened);
            }
        }
        
    }
}
