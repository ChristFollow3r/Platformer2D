




using System;
using UnityEngine;

namespace Items
{
    public record Slot
    {
        public int id;
        public bool isEmpty => item is null;
        public bool isFull => IsFull();
        public ItemStack item = null;

        public int GetCapacity(ItemStack itemToCheck, out int surplus)
        {
            #region GetCapacity
            surplus = 0;
            if (item == null) return 0;


            int available = item.data.stack - item.amount;
            surplus = Math.Max(0, itemToCheck.amount - available);
            int amountToAdd = itemToCheck.amount - surplus;

            return amountToAdd;
            #endregion
        }

        private bool IsFull()
        {
            #region IsFull
            if (isEmpty) return false;
            return item.amount >= item.data.stack;
            #endregion
        }

        public void Add(ItemStack itemToAdd)
        {
            #region Add
            if (itemToAdd == null) return;
            if (isEmpty)
            {
                item = new ItemStack(itemToAdd.data) { amount = itemToAdd.amount };
                itemToAdd.amount = 0;
                return;
            }

            int amountToAdd = GetCapacity(itemToAdd, out int surplus);
            item.amount += (short)amountToAdd;
            itemToAdd.amount = (short)surplus;
            #endregion
        }
    }
}
