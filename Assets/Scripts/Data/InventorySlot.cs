using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Data
{
    public class InventorySlot
    {
        public Item item;
        public int amount = 0;

        public bool IsEmpty => item is null || amount <= 0;
        public bool IsFull => item is not null && amount >= item.maxStack;

        public bool CanBeStacked(Item otherItem) => item is not null && otherItem is not null && item == otherItem;

        public int AddItem(Item newItem, int amountToAdd)
        {
            if (IsEmpty)
            {
                item = newItem;
                amount = Mathf.Min(amountToAdd, item.maxStack); // Returns the smallest number
                return amountToAdd - amount;
            }

            if (CanBeStacked(newItem))
            {
                int spaceLeft = item.maxStack - amount;
                int newAmountToAdd = Mathf.Min(spaceLeft, amountToAdd);
                
                amount += newAmountToAdd;
                return amountToAdd - newAmountToAdd;
            }
                
            return amountToAdd;
        }
        
    }
}
