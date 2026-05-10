using System;
using System.Linq;
using Items.Utils;


namespace Items.Overlays
{

  public enum EquipmentType
  {
    Helmet,
    Chest,
    OffHand,
    Pants,
    Bots
  }


  public class Equipment : Overlay, IInventory
  {
    #region Data
    public const short EquipmentSlots = 5;
    public const short CraftingSlots = 4;
    public Slot[] equipmentSlots = new Slot[EquipmentSlots];
    public Slot[] craftingSlots = new Slot[CraftingSlots];
    public Slot resultSlot;
    #endregion

    #region Events
    public event Action<int, ItemStack> OnSlotChanged;
    #endregion

    #region Contructor
    public Equipment() : base(-1)
    {
      Init();
    }
    #endregion

    #region Methods
    private void Init()
    {
      #region Init
      for (int i = 0; i < equipmentSlots.Length; i++)
      {
        equipmentSlots[i] = new Slot() { id = i };
      }
      for (int i = 0; i < craftingSlots.Length; i++)
      {
        craftingSlots[i] = new Slot() { id = EquipmentSlots + i };
      }
      resultSlot = new Slot() { id = EquipmentSlots + CraftingSlots };
      #endregion
    }

    public ItemStack AddEquipment(EquipmentType equipmentType, ItemStack itemStack)
    {
      #region AddEquipment
      Slot slot = equipmentSlots[(int)equipmentType];
      ItemStack prev = slot.item;

      slot.item = itemStack;
      OnSlotChanged?.Invoke(slot.id, slot.item);
      return prev;
      #endregion
    }

    public bool AddToCraftingSlot(int slotId, ItemStack itemStack)
    {
      #region AddToCraftingSlot
      if (slotId < 0 || slotId >= CraftingSlots) return false;

      Slot slot = craftingSlots[slotId];
      if (!slot.isEmpty && slot.item.data != itemStack.data) return false;

      slot.Add(itemStack);
      OnSlotChanged?.Invoke(slot.id, slot.item);

      ItemStack result = CraftingUtils.EvaluateCraft(craftingSlots.Select(s => s.item).ToList());
      resultSlot.item = null;
      resultSlot.Add(result);
      OnSlotChanged?.Invoke(resultSlot.id, resultSlot.item);
      return true;
      #endregion
    }

    public void Add(ItemStack itemStack) => Inventory.Singleton.Drop(itemStack);

    public bool AddToSlot(ItemStack itemStack, int slotId)
    {
      #region AddToSlot
      return false;
      #endregion
    }
    public bool RemoveAmount(int slotId, short amount)
    {
      #region RemoveAmount
      return false;
      #endregion
    }

    public ItemStack ClearSlot(int slotId)
    {
      #region ClearSlot
      return null;
      #endregion
    }
    #endregion
  }
}
