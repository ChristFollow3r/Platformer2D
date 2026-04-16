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
    
        // Add global inventory method (loop through arrays and call AddItem() when needed

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
            
            AddItemToInventary(item, amount);
            
        }

        public void AddItemToInventary(Item item, int amount)
        {
            return;
        }
    
        // Add two getters for the arrays
    }
}
