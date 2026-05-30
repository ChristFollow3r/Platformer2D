using System;
using System.Linq;
using Items.Utils;
using Player;
using Scriptable_Objects_Scripts;
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
        public float fillPercent
        {
            get => _fillPercent;
            private set
            {
                _fillPercent = value;
                OnFillChanged?.Invoke(_fillPercent);
            }
        }
        private float _fillPercent = 0;

        private ItemStack currentResult = null;
        private float currentCookDuration = 0;
        private float currentCookTimer = 0;
        #endregion


        #region Events
        public event Action<int, ItemStack> OnSlotChanged;
        public event Action<float> OnFillChanged;
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
            ItemStack result = CookingUtils.EvaluateCook(cookingSlots.Select(s => s.item).ToList(), out CookingRecipe cookingRecipe);
            if (result == null)
            {
                currentCookDuration = 0;
                currentCookTimer = 0;
                fillPercent = 0;
                currentResult = null;
                return false;
            }

            if (!resultSlot.isEmpty && result.data != resultSlot.item.data) return false;
            if (!resultSlot.isEmpty && resultSlot.item.amount >= resultSlot.item.data.stack)
                return false;

            if (currentResult == null || result.data != currentResult.data)
            {
                currentCookDuration = cookingRecipe.cookTime;
                currentCookTimer = 0;
                fillPercent = 0;
                currentResult = result;
                return true;
            }

            Debug.Log($"Same recipe!, resetting stuff");
            currentCookTimer = 0;
            fillPercent = 0;
            currentResult = result;
            Debug.Log($"Current result {currentResult.amount}");
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
            if (slot.id != resultSlot.id) EvaluateCook();
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

            EvaluateCook();

            return itemStack;
            #endregion
        }

        public override void Tick()
        {
            if (currentResult == null) return;
            currentCookTimer += Time.deltaTime;
            fillPercent = Mathf.Clamp01(currentCookTimer / currentCookDuration);
            if (currentCookTimer >= currentCookDuration)
            {
                Debug.Log($"adding {currentResult.amount}");
                resultSlot.Add(currentResult);
                OnSlotChanged?.Invoke(resultSlot.id, resultSlot.item);
                EvaluateCook();
                for (int i = 0; i < CookingSlots; i++)
                {
                    if (cookingSlots[i].isEmpty) continue;
                    RemoveAmount(cookingSlots[i].id, 1);
                }
            }
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
