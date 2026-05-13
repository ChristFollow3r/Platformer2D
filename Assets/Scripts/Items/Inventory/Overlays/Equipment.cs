using System;
using System.Linq;
using Items.Utils;
using UnityEngine;


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
    public const short MaxSlotId = EquipmentSlots + CraftingSlots;
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

    public bool EvaluateCraft()
    {
      #region EvaluateCraft
      Debug.Log("Evaluating craft!");
      ItemStack result = CraftingUtils.EvaluateCraft(craftingSlots.Select(s => s.item).ToList(), 2);

      resultSlot.item = null;
      if (result != null) resultSlot.Add(result);
      OnSlotChanged?.Invoke(resultSlot.id, resultSlot.item);

      // Add callback to item pickup?
      return true;
      #endregion
    }

    public void Add(ItemStack itemStack) => Inventory.Singleton.Drop(itemStack);

    public bool AddToSlot(ItemStack itemStack, int slotId)
    {
      #region AddToSlot
      if (slotId < 0 || slotId > MaxSlotId)
      {
        Debug.LogWarning($"Tried to add to out-of-range slot {slotId}");
        return false;
      }
      bool isCraftingSlot = slotId >= EquipmentSlots && slotId < EquipmentSlots + CraftingSlots;
      Slot slot = isCraftingSlot ? craftingSlots[slotId - EquipmentSlots] : equipmentSlots[slotId];

      if (!slot.isEmpty && slot.item.data != itemStack.data) return false;
      slot.Add(itemStack);
      OnSlotChanged?.Invoke(slotId, slot.item);
      if (isCraftingSlot) EvaluateCraft();
      return true;
      #endregion
    }
    public bool RemoveAmount(int slotId, short amount)
    {
      #region RemoveAmount
      if (slotId < 0 || slotId > MaxSlotId)
      {
        Debug.LogWarning($"Tried to add to out-of-range slot {slotId}");
        return false;
      }
      Slot slot;
      if (slotId < EquipmentSlots) slot = equipmentSlots[slotId];
      else slot = craftingSlots[slotId - EquipmentSlots];

      slot.item.amount -= amount;
      if (slot.item.amount == 0) ClearSlot(slotId);
      else OnSlotChanged(slotId, slot.item);
      return true;
      #endregion
    }

    public ItemStack ClearSlot(int slotId)
    {
      #region ClearSlot
      Slot slot;
      bool isCraftingSlot = false;

      if (slotId == resultSlot.id) slot = resultSlot;
      else
      {
        isCraftingSlot = slotId >= EquipmentSlots && slotId < EquipmentSlots + CraftingSlots;
        slot = isCraftingSlot ? craftingSlots[slotId - EquipmentSlots] : equipmentSlots[slotId];
      }

      if (slot.isEmpty) return null;

      ItemStack itemStack = slot.item;
      slot.item = null;

      OnSlotChanged?.Invoke(slotId, null);

      if (isCraftingSlot) EvaluateCraft();
      return itemStack;
      #endregion
    }

    protected override void CloseOverlay()
    {
      #region OnOverlayClose
      foreach (Slot slot in craftingSlots)
      {
        if (slot.isEmpty) continue;
        Inventory.Singleton.Add(slot.item);
        slot.item = null;
      }
      #endregion
    }

    /// <summary>Method</summary>
    public override void RefreshUI()
    {
      #region RefreshUI
      foreach (Slot slot in equipmentSlots)
      {
        if (slot.isEmpty) continue;
        OnSlotChanged?.Invoke(slot.id, slot.item);
      }
      #endregion
    }
    #endregion
  }
}
