using Data.Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class SlotUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI amountText;

        private InventorySlot linkedSlot;

        public void SetSlot(InventorySlot slot)
        {
            if (linkedSlot != null) linkedSlot.OnSlotChanged -= UpdateVisuals;
            linkedSlot = slot;
            linkedSlot.OnSlotChanged += UpdateVisuals;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (linkedSlot.IsEmpty)
            {
                iconImage.enabled = false;
                amountText.text = "";
            }
            else
            {
                iconImage.enabled = true;
                iconImage.sprite = linkedSlot.GetItem().itemIcon;
                amountText.text = linkedSlot.GetAmount().ToString();
                if (linkedSlot.GetAmount() == 1) amountText.text = "";
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                InventoryManager.Instance.HandleLeftClick(linkedSlot);
            else if (eventData.button == PointerEventData.InputButton.Right)
                InventoryManager.Instance.HandleRightClick(linkedSlot);
        }
    }
}
