
namespace Items
{
  public interface IInventory
  {
    public void Add(ItemStack itemStack);
    public bool AddToSlot(ItemStack itemStack, int slotId);
    public bool RemoveAmount(int slotId, short amount);
    public ItemStack ClearSlot(int slotId);
  }
}
