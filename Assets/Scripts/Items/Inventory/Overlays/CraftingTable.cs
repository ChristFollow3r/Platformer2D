using System;
using System.Linq;
using Items.Utils;
using UnityEngine;


namespace Items.Overlays
{


    [Serializable]
    public class CraftingTable : Overlay, IInventory
    {
        #region Data
        public const short CraftingSlots = 16;
        public const short MaxSlotId = CraftingSlots;
        public Slot[] craftingSlots = new Slot[CraftingSlots];
        public Slot resultSlot;
        #endregion

        #region Events
        public event Action<int, ItemStack> OnSlotChanged;
        #endregion

        #region Contructor
        public CraftingTable(ulong blockId) : base(blockId, Player.OverlayType.CraftingTable)
        {
            Init();
        }
        #endregion

        #region Methods
        private void Init()
        {
            #region Init
            for (int i = 0; i < craftingSlots.Length; i++)
            {
                craftingSlots[i] = new Slot() { id = i };
            }
            resultSlot = new Slot() { id = CraftingSlots };
            #endregion
        }



        public bool EvaluateCraft()
        {
            #region EvaluateCraft
            ItemStack result = CraftingUtils.EvaluateCraft(craftingSlots.Select(s => s.item).ToList(), 4);
            Debug.Log($"EvaluateCraft result: {result?.data?.name ?? "null"}");
            resultSlot.item = null;
            if (result != null) resultSlot.Add(result);
            OnSlotChanged?.Invoke(resultSlot.id, resultSlot.item);

            return true;
            #endregion
        }

        public void Add(ItemStack itemStack, bool stacked = true) => Inventory.Singleton.Drop(itemStack);

        public bool AddToSlot(ItemStack itemStack, int slotId)
        {
            #region AddToSlot
            if (slotId < 0 || slotId > MaxSlotId)
            {
                Debug.LogWarning($"Tried to add to out-of-range slot {slotId}");
                return false;
            }

            bool isResultSlot = slotId == resultSlot.id;

            Slot slot = isResultSlot ? resultSlot : craftingSlots[slotId];

            if (!slot.isEmpty && slot.item.data != itemStack.data) return false;
            slot.Add(itemStack);
            OnSlotChanged?.Invoke(slotId, slot.item);
            if (!isResultSlot) EvaluateCraft();
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
            Slot slot = slotId == resultSlot.id ? resultSlot : craftingSlots[slotId];

            slot.item.amount -= amount;
            if (slot.item.amount <= 0) ClearSlot(slotId);
            else OnSlotChanged(slotId, slot.item);
            return true;
            #endregion
        }

        public ItemStack ClearSlot(int slotId)
        {
            #region ClearSlot
            bool isResultSlot = slotId == resultSlot.id;
            Slot slot = isResultSlot ? resultSlot : craftingSlots[slotId];

            if (slot.isEmpty) return null;

            ItemStack itemStack = slot.item;
            slot.item = null;

            OnSlotChanged?.Invoke(slotId, null);

            if (isResultSlot)
            {
                for (int i = 0; i < CraftingSlots; i++)
                {
                    if (craftingSlots[i].isEmpty) continue;
                    RemoveAmount(craftingSlots[i].id, 1);
                }
            }

            EvaluateCraft();
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

        public string ToJson()
        {
            CloseOverlay();
            return "{}";
        }
        public void FromJson(string json) { }
        #endregion
    }
}
