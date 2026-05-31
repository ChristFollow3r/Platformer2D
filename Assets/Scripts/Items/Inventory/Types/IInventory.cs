
using System;

namespace Items
{
    public interface IInventory
    {
        public event Action<int, ItemStack> OnSlotChanged;
        public void Add(ItemStack itemStack, bool stacked = true);
        public bool AddToSlot(ItemStack itemStack, int slotId);
        public bool RemoveAmount(int slotId, short amount);
        public ItemStack ClearSlot(int slotId);
        public string ToJson();
        public void FromJson(string json);
    }
}
