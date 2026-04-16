using Scriptable_Objects_Scripts;
using System.Linq;

namespace data
{
    public class Inventory
    {
        private Data.InventorySlot[,] inventorySlots =  new Data.InventorySlot[3, 9];
        private Data.InventorySlot[] hotBarSlots = new Data.InventorySlot[9];

        public Inventory()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    inventorySlots[i, j] = new Data.InventorySlot();
                }
            }

            for (int i = 0; i < 9; i++)
            {
                hotBarSlots[i] = new Data.InventorySlot();
            }
        }

        public void AddItemToHotbar(Item item, int amount)
        {
            if (item is null || amount <= 0) return;

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
    
        // Add two getters for the arrays
    }
}
