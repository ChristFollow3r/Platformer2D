using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI amountText;

    private Data.InventorySlot linkedSlot;

    public void SetSlot(Data.InventorySlot slot)
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
        // To do the drag and drop
    }
}
