using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

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
        

        public void HandleLeftClick(InventorySlot clickedSlot)
        {
            if (mouseSlot.IsEmpty && !clickedSlot.IsEmpty) // Take all - WORKS
            {
                mouseSlot.AddItem(clickedSlot.GetItem(), clickedSlot.GetAmount());
                clickedSlot.Remove(clickedSlot.GetAmount());
                Debug.Log("Working one");
            }

            else if (!mouseSlot.IsEmpty && clickedSlot.IsEmpty) // Put all back - WORKS
            {
                clickedSlot.AddItem(mouseSlot.GetItem(), mouseSlot.GetAmount());
                mouseSlot.Remove(mouseSlot.GetAmount());
                Debug.Log("Working two");
            }
            
            else if (!mouseSlot.IsEmpty && !clickedSlot.IsEmpty) // Both have stuff
            {
                if (clickedSlot.CanBeStacked(mouseSlot.GetItem())) // Add stuff to mouse with stuff - WORKS
                {
                    int mouseAmount = mouseSlot.GetAmount();
                    int amountLeft = clickedSlot.AddItem(mouseSlot.GetItem(), mouseAmount);
                    int amountToRemove = mouseAmount - amountLeft;
                    mouseSlot.Remove(amountToRemove);
                }

                else // Swap - WORKS
                {
                    var slotItem = clickedSlot.GetItem();
                    var  slotAmount = clickedSlot.GetAmount();
                    
                    var mouseItem = mouseSlot.GetItem();
                    var mouseAmount = mouseSlot.GetAmount();
                    
                    clickedSlot.Remove(clickedSlot.GetAmount());
                    mouseSlot.Remove(mouseSlot.GetAmount());
                    
                    clickedSlot.AddItem(mouseItem, mouseAmount);
                    mouseSlot.AddItem(slotItem, slotAmount);
                }
            }
            
            MouseGhostVisuals(); // TO FIX ITEMS MAKE THE PLAYER MOVEMENT SCRIPT THINK THEY ARE WALLS

        }

        public void HandleRightClick(InventorySlot clickedSlot)
        {
            if (clickedSlot.IsEmpty) return;
            
            if (mouseSlot.IsEmpty || (mouseSlot.CanBeStacked(clickedSlot.GetItem()) && !mouseSlot.IsFull))
            {
                mouseSlot.AddItem(clickedSlot.GetItem(), 1);
                clickedSlot.Remove(1);
                Debug.Log("Took one");
            }
            
            else Debug.Log("Mouse is full or holding something else");
            MouseGhostVisuals();
        }

        public void DropItem()
        {
            if (!mouseSlot.IsEmpty)
            {
                Instantiate(mouseSlot.GetItem().drop, mouseGhost.transform.position, Quaternion.identity);
                mouseSlot.Remove(1);
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
