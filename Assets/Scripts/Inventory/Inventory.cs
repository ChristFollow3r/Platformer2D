

using System;
using Data;
using Scriptable_Objects_Scripts;

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
    public static event Action<int, Item> OnSlotChanged;
    #endregion


    #region Methods
    /// <summary>Method</summary>
    static public void Add(Item item)
    {
      #region Pickup
      bool drop = false;
      do
      {
        Slot slot = GetSlotOfItem(item.itemData);
        if (slot == null) { drop = true; break; }
        slot.Add(item);
        OnSlotChanged?.Invoke(slot.id, slot.item);
      } while (item.amount > 0);

      if (!drop) return;

      Drop(item);
      #endregion
    }
    /// <summary>Method</summary>
    public static void Drop(Item item)
    {
      #region Drop
      // TODO
      #endregion
    }

    /// <summary>Method</summary>
    private static Slot GetSlotOfItem(ItemData itemData)
    {
      #region GetSlotOfItem
      foreach (Slot slot in items)
      {
        if (slot.item.itemData == itemData && !slot.isFull) return slot;
      }
      return null;
      #endregion
    }
    #endregion
  }
}
