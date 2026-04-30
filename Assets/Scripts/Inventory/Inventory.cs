

using System;
using Data;

namespace Items
{

  public static class Inventory
  {

    #region Data
    public const short HotbarItems = 10;
    public const short Rows = 10;
    public const short Cols = 10;
    public static Slot[] items = new Slot[HotbarItems + Cols + Rows];
    #endregion

    #region Events
    public static event Action<int, ItemStack> OnSlotChanged;
    #endregion


    #region Methods
    static public void Add(ItemStack item)
    {
      #region Pickup

      Slot slot;
      do
      {
        slot = GetSlotOfItem(item.data);
        if (slot is null) break;
        slot.Add(item);
        OnSlotChanged?.Invoke(slot.id, slot.item);
      } while (item.amount > 0);

      if (slot is not null) return;

      Drop(item);
      #endregion
    }

    public static void Drop(ItemStack item)
    {
      #region Drop
      // TODO
      #endregion
    }

    private static Slot GetSlotOfItem(ItemData itemData)
    {
      #region GetSlotOfItem
      foreach (Slot slot in items)
      {
        if (slot.item.data == itemData && !slot.isFull) return slot;
      }
      return null;
      #endregion
    }
    #endregion
  }
}
