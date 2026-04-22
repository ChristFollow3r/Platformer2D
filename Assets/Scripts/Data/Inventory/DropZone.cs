using UnityEngine;
using UnityEngine.EventSystems;

namespace Data.Inventory
{
    public class DropZone : MonoBehaviour, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                InventoryManager.Instance.DropItem();
            }
        }
    }
}
