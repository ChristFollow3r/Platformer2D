using System;
using System.Linq;
using Items.Utils;
using UnityEngine;


namespace Items.Overlays
{

    public enum EquipmentType
    {
        Mod,
    }

    public enum Mod
    {
        Stone,
        Copper,
        Bronze,
        Iron,
        Primordial
    }


    [Serializable]
    public class Equipment : Overlay, IInventory
    {
        #region Data
        public const short EquipmentSlots = 1;
        public const short CraftingSlots = 4;
        public const short MaxSlotId = EquipmentSlots + CraftingSlots;
        public Slot[] equipmentSlots = new Slot[EquipmentSlots];
        public Slot[] craftingSlots = new Slot[CraftingSlots];
        public Slot resultSlot;
        public static Equipment Singleton;
        #endregion

        #region Events
        public event Action<int, ItemStack> OnSlotChanged;
        public event Action<bool, Mod> OnModChange;
        #endregion

        #region Contructor
        public Equipment() : base(ulong.MinValue, Player.OverlayType.Inventory)
        {
            Init();
        }
        #endregion

        #region Methods
        private void Init()
        {
            #region Init
            Singleton = this;
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


        public override void Tick()
        {
            #region Tick
            ItemStack mod = equipmentSlots[(int)EquipmentType.Mod].item;
            if (mod == null) return;

            mod.duration -= Time.deltaTime;
            if (mod.duration >= 0) return;
            ClearSlot((int)EquipmentType.Mod);
            OnModChange?.Invoke(false, 0);
            #endregion
        }

        public ItemStack AddEquipment(EquipmentType equipmentType, ItemStack itemStack)
        {
            #region AddEquipment
            Slot slot = equipmentSlots[(int)equipmentType];
            ItemStack prev = slot.item;

            slot.item = itemStack;
            OnSlotChanged?.Invoke(slot.id, slot.item);

            if (equipmentType == EquipmentType.Mod) OnModChange?.Invoke(true, itemStack.data.modData.mod);
            return prev;
            #endregion
        }

        public bool EvaluateCraft()
        {
            #region EvaluateCraft
            ItemStack result = CraftingUtils.EvaluateCraft(craftingSlots.Select(s => s.item).ToList(), 2);

            resultSlot.item = null;
            if (result != null) resultSlot.Add(result);
            OnSlotChanged?.Invoke(resultSlot.id, resultSlot.item);

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
            bool isEquipmentSlot = slotId < EquipmentSlots;
            if (isEquipmentSlot)
            {

                if (!itemStack.data.isConsumable) return false;
                if ((int)itemStack.data.equipmentType != slotId) return false;
                ItemStack prev = AddEquipment(itemStack.data.equipmentType, itemStack);
                if (prev != null) Inventory.Singleton.Add(prev);
                return true;
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
            if (slot.item.amount <= 0) ClearSlot(slotId);
            else OnSlotChanged(slotId, slot.item);
            return true;
            #endregion
        }

        public ItemStack ClearSlot(int slotId)
        {
            #region ClearSlot
            Slot slot;
            bool isCraftingSlot = false;
            bool isResultSlot = slotId == resultSlot.id;



            if (isResultSlot) slot = resultSlot;
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
            if (isResultSlot)
            {
                for (int i = 0; i < CraftingSlots; i++)
                {
                    if (craftingSlots[i].isEmpty) continue;
                    RemoveAmount(craftingSlots[i].id, 1);
                }
            }

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

        public float GetMiningPower()
        {
            #region GetMiningPower
            ItemStack modItem = equipmentSlots[(int)EquipmentType.Mod].item;
            if (modItem == null) return 1f;
            if (modItem.data.modData == null) return 1f;
            return modItem.data.modData.minigPower;
            #endregion
        }
        #endregion
    }
}
