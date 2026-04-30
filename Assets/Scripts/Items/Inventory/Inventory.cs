

using System;
using Data;
using UnityEngine;

namespace Items
{

  public static class Inventory
  {

    #region Data
    public const short HotbarItems = 10;
    public const short Rows = 10;
    public const short Cols = 10;
    public static Slot[] slots = new Slot[HotbarItems + Cols + Rows];
    #endregion

    #region Events
    public static event Action<int, ItemStack> OnSlotChanged;
    #endregion


    #region Methods
    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
      #region Init
      for (int i = 0; i < slots.Length; i++)
      {
        slots[i] = new Slot()
        {
          id = i
        };
      }
      #endregion
    }

    public static void Add(ItemStack item)
    {
      #region Pickup
      Slot slot;
      do
      {
        slot = GetSlotOfItem(item.data);
        if (slot is null) break;
        Debug.Log($"Adding {item.amount} to slot {slot.id}");
        slot.Add(item);
        Debug.Log($"New amount is {item.amount}");

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


    public static ItemStack ClearSlot(int slotId)
    {
      #region ClearSlot
      Slot slot = slots[slotId];
      if (slot.isEmpty) return null;

      ItemStack itemStack = slot.item;
      slot.item = null;

      OnSlotChanged?.Invoke(slotId, null);
      return itemStack;
      #endregion
    }

    public static void AddToSlot(ItemStack item, int slotId)
    {
      #region AddToSlot
      Slot slot = slots[slotId];
      if (!slot.isEmpty && slot.item.data != item.data) return;

      slot.Add(item);
      OnSlotChanged?.Invoke(slotId, null);
      #endregion
    }

    private static Slot GetSlotOfItem(ItemData itemData)
    {
      #region GetSlotOfItem
      foreach (Slot slot in slots)
      {
        Debug.Log(slot);
        if (slot.isEmpty || (slot.item.data == itemData && !slot.isFull)) return slot;
      }
      return null;
      #endregion
    }
    #endregion
  }
}
