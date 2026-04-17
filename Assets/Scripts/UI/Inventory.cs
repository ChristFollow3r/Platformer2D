using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private GameObject hotbar;
        [SerializeField] private Player.PlayerManager playerManager;

        private void Start()
        {
            foreach (var slot in playerManager.Inventory.GetHotBarSlots())
                slot.OnSlotChanged += UpdateHotbarUI;
        }

        private void UpdateHotbarUI()
        {
            var hotBarSlots = playerManager.Inventory.GetHotBarSlots();
            
            for (int i = 0; i < 9; i++)
            {
                Debug.Log("Hotbar Slot: " + hotBarSlots[i] +  "being filled");
                if (hotBarSlots[i] is null) continue; // This is unnecessary ?
                Transform hotBarSlot = hotbar.transform.GetChild(i);
                if (hotBarSlots[i].GetItem() is null) continue;
                Sprite itemSprite = hotBarSlots[i].GetItem().itemIcon;
                hotBarSlot.GetChild(0).GetComponent<Image>().sprite = itemSprite;
            }
        }
    }
}
