




using System;

namespace Items
{
  public record Slot
  {
    public int id;
    public bool isEmpty => item is null;
    public bool isFull => IsFull();
    public Item item;

    /// <summary>Method</summary>
    public int HowMayFit(Item itemToCheck, out int surplus)
    {
      #region DoesFit
      surplus = 0;
      if (item == null) return -1;


      int available = item.itemData.stack - item.amount;
      surplus = Math.Max(0, itemToCheck.amount - available);
      int amountToAdd = itemToCheck.amount - surplus;

      return amountToAdd;
      #endregion
    }

    /// <summary>Method</summary>
    private bool IsFull()
    {
      #region IsFull
      return item.amount >= item.itemData.stack;
      #endregion
    }

    /// <summary>Method</summary>
    public void Add(Item itemToAdd)
    {
      #region Add
      int amountToAdd = HowMayFit(itemToAdd, out int surplus);
      item.amount += (short)amountToAdd;
      itemToAdd.amount -= (short)surplus;
      #endregion
    }
  }
}
