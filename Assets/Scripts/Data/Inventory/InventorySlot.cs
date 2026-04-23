using System;
using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Data.Inventory
{
    public class InventorySlot
    {
        private Item item;
        private int amount = 0;

        public event Action OnSlotChanged;
        public bool IsEmpty => item is null || amount <= 0;
        public bool IsFull => item is not null && amount >= item.maxStack;
        public bool CanBeStacked(Item otherItem) => item is not null && otherItem is not null && item == otherItem;
        
        public Item GetItem() => item;
        public int GetAmount() => amount;

        public int AddItem(Item newItem, int amountToAdd)
        {
            if (IsEmpty)
            {
                item = newItem;
                amount = Mathf.Min(amountToAdd, item.maxStack); // Returns the smallest number
                OnSlotChanged?.Invoke();
                return amountToAdd - amount;
            }

            if (CanBeStacked(newItem))
            {
                int spaceLeft = item.maxStack - amount;
                int newAmountToAdd = Mathf.Min(spaceLeft, amountToAdd);
                
                amount += newAmountToAdd;
                OnSlotChanged?.Invoke();
                return amountToAdd - newAmountToAdd;
            }
                
            return amountToAdd;
        }

        public int Remove(int amountToRemove)
        {
            if (!IsEmpty)
            {
                int removed = Mathf.Min(amountToRemove, amount);
                amount -= removed;
                if (amount <= 0) ClearSlot();
                OnSlotChanged?.Invoke();
                return (removed);
            }

            return 0;
        }
        
        private void ClearSlot()
        {
            item = null;
            amount = 0;
        }
        
    }
}
