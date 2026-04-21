using Scriptable_Objects_Scripts;
using UnityEngine;

namespace Data.Inventory
{
    public class Inventory
    {
        private readonly InventorySlot[,] inventorySlots =  new InventorySlot[3, 9];
        private readonly InventorySlot[] hotBarSlots = new InventorySlot[9];

        public Inventory()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    inventorySlots[i, j] = new InventorySlot();
                }
            }

            for (int i = 0; i < 9; i++)
            {
                hotBarSlots[i] = new InventorySlot();
            }
        }

        public void AddItemToHotbar(Item item, int amount)
        {
            if (item is null || amount <= 0) return;
            Debug.Log("Adding Item to Hotbar");
            
            for (int i = 0; i < 9; i++)
            {
                if (!hotBarSlots[i].CanBeStacked(item)) 
                    continue;
                amount = hotBarSlots[i].AddItem(item, amount);
                if (amount <= 0) return;
            }

            for (int i = 0; i < 9; i++)
            {
                if (!hotBarSlots[i].IsEmpty)
                    continue;
                amount = hotBarSlots[i].AddItem(item, amount);
                if (amount <= 0) return;
            }
            
            AddItemToInventory(item, amount);
            
        }

        private void AddItemToInventory(Item item, int amount)
        {
            if (item is null || amount <= 0) return;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (!inventorySlots[i, j].CanBeStacked(item))
                        continue;
                    amount = inventorySlots[i, j].AddItem(item, amount);
                    if (amount <= 0) return;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (!inventorySlots[i, j].IsEmpty)
                        continue;
                    amount = inventorySlots[i, j].AddItem(item, amount);
                    if (amount <= 0) return;
                }
            }
        }

        public InventorySlot[] GetHotBarSlots()
        {
            return hotBarSlots;
        }

        public InventorySlot[,] GetInventorySlots()
        {
            return inventorySlots;
        }

    }
}
