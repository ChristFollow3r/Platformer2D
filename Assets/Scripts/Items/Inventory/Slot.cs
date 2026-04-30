




using System;

namespace Items
{
  public record Slot
  {
    public int id;
    public bool isEmpty => item is null;
    public bool isFull => IsFull();
    public ItemStack item;

    public int GetCapacity(ItemStack itemToCheck, out int surplus)
    {
      #region GetCapacity
      surplus = 0;
      if (item == null) return -1;


      int available = item.data.stack - item.amount;
      surplus = Math.Max(0, itemToCheck.amount - available);
      int amountToAdd = itemToCheck.amount - surplus;

      return amountToAdd;
      #endregion
    }

    private bool IsFull()
    {
      #region IsFull
      if (isEmpty) return false;
      return item.amount >= item.data.stack;
      #endregion
    }

    public void Add(ItemStack itemToAdd)
    {
      #region Add
      if (isEmpty)
      {
        item = new ItemStack()
        {
          data = itemToAdd.data,
        };
      }

      int amountToAdd = GetCapacity(itemToAdd, out int surplus);
      item.amount += (short)amountToAdd;
      itemToAdd.amount = (short)surplus;
      #endregion
    }
  }
}
