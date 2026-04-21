using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Data.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private GameObject mouseGhost;
        [SerializeField] private Image ghostIcon;
        [SerializeField] private TextMeshProUGUI ghostText;
    
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
        

        public void HandleClick(InventorySlot clickedSlot)
        {
            if (mouseSlot.IsEmpty && !clickedSlot.IsEmpty) // Regular subtract - nothing on the mouse - slot occupied
            {
                mouseSlot.AddItem(clickedSlot.GetItem(), clickedSlot.GetAmount());
                clickedSlot.Remove(clickedSlot.GetAmount());
            }

            else if (!mouseSlot.IsEmpty && clickedSlot.IsEmpty) // Regular transfer - mouse has stuff - slot has nothing
            {
                var temp = mouseSlot.GetItem();
                int tempAmount = mouseSlot.GetAmount();

                clickedSlot.AddItem(temp, tempAmount);
                mouseSlot.Remove(tempAmount);
            }
            
            // add items to a slot (individual items)
            // subtract items from a slot (individual items)
            // drop items with left click
            
            MouseGhostVisuals(); // TO FIX ITEMS MAKE THE PLAYER MOVEMENT SCRIPT THINK THEY ARE WALLS

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
