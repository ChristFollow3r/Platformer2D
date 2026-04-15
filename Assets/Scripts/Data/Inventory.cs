
public class Inventory
{
    private Data.InventorySlot[,] slots =  new Data.InventorySlot[3, 9];
    private Data.InventorySlot[] hotBarSlots = new Data.InventorySlot[9];

    public Inventory()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                slots[i, j] = new Data.InventorySlot();
            }
        }

        for (int i = 0; i < 9; i++)
        {
            hotBarSlots[i] = new Data.InventorySlot();
        }
    }
    
    // Add global inventory method (loop through arrays and call AddItem() when needed
    
    // Add two getters for the arrays
}
