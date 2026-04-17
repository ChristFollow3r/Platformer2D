using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private GameObject hotbar;
        [SerializeField] private Player.PlayerManager playerManager;

        private void Awake()
        {
            foreach (var slot in playerManager.Inventory.GetHotBarSlots())
                slot.OnSlotChanged += UpdateHotbarUI;
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
    }
}
