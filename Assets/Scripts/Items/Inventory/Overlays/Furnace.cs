using System;
using System.Linq;
using Items.Utils;
using Player;
using UnityEngine;


namespace Items.Overlays
{



    [Serializable]
    public class Furnace : Overlay, IInventory
    {
        #region Data
        public const short CookingSlots = 4;
        public const short MaxSlotId = CookingSlots;
        public Slot[] cookingSlots = new Slot[CookingSlots];
        public Slot resultSlot;
        #endregion

        #region Events
        public event Action<int, ItemStack> OnSlotChanged;
        #endregion

        #region Contructor
        public Furnace(ulong blockId) : base(blockId, OverlayType.Furnace)
        {
            Init();
        }
        #endregion

        #region Methods
        private void Init()
        {
            #region Init

            for (int i = 0; i < cookingSlots.Length; i++)
            {
                cookingSlots[i] = new Slot() { id = i };
            }
            resultSlot = new Slot() { id = CookingSlots };
            #endregion
        }


        public bool EvaluateCook()
        {
            #region EvaluateCook
            ItemStack result = CookingUtils.EvaluateCook(cookingSlots.Select(s => s.item).ToList());

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

            Slot slot = cookingSlots[slotId];

            if (!slot.isEmpty && slot.item.data != itemStack.data) return false;
            slot.Add(itemStack);
            OnSlotChanged?.Invoke(slotId, slot.item);
            EvaluateCook();
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
            Slot slot = cookingSlots[slotId];

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
            bool isResultSlot = slotId == resultSlot.id;

            if (isResultSlot) slot = resultSlot;
            else slot = cookingSlots[slotId];


            if (slot.isEmpty) return null;

            ItemStack itemStack = slot.item;
            slot.item = null;

            OnSlotChanged?.Invoke(slotId, null);

            if (!isResultSlot) EvaluateCook();

            return itemStack;
            #endregion
        }

        protected override void CloseOverlay()
        {
            #region OnOverlayClose
            #endregion
        }

        /// <summary>Method</summary>
        public override void RefreshUI()
        {
            #region RefreshUI
            foreach (Slot slot in cookingSlots)
            {
                if (slot.isEmpty) continue;
                OnSlotChanged?.Invoke(slot.id, slot.item);
            }
            if (resultSlot.isEmpty) return;
            OnSlotChanged?.Invoke(resultSlot.id, resultSlot.item);
            #endregion
        }
        #endregion
    }
}
