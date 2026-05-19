

using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Items
{

    [DefaultExecutionOrder(-100)]
    public class Inventory : MonoBehaviour, IInventory
    {
        #region Singleton setup
        public static Inventory Singleton;
        private void SetupSingleton()
        {
            #region SetupSingleton
            if (Singleton != null && Singleton != this) { Destroy(gameObject); return; }
            Singleton = this;
            #endregion
        }
        #endregion

        #region Data
        public const short HotbarSlots = 10;
        public const short RowSlots = 6;
        public const short ColSlots = 5;
        public Slot[] slots = new Slot[HotbarSlots + ColSlots * RowSlots];
        public ItemStack hand => slots[handIndex].item;
        public short handIndex
        {
            get => _handIndex; set
            {
                _handIndex = value;
                if (_handIndex == HotbarSlots) _handIndex = 0;
                if (_handIndex < 0) _handIndex = HotbarSlots - 1;

                OnHandChanged?.Invoke(_handIndex);
            }
        }
        private short _handIndex = 0;
        [SerializeField] private List<ItemStack> startingItems = new();
        #endregion

        #region Events
        public event Action<int, ItemStack> OnSlotChanged;
        public event Action<short> OnHandChanged;
        #endregion


        #region Unity
        /// <summary>Ran by unity on load</summary>
        private void Awake()
        {
            #region Awake
            SetupSingleton();
            CreateSlots();
            #endregion
        }

        /// <summary>Ran by unity on first enable</summary>
        private void Start()
        {
            #region Start
            foreach (ItemStack itemStack in startingItems)
            {
                Add(itemStack);
            }
            #endregion
        }
        #endregion

        #region Methods
        private void CreateSlots()
        {
            #region CreateElements
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new Slot() { id = i };
            }
            #endregion
        }

        public void Add(ItemStack item)
        {
            #region Pickup
            Slot slot;
            do
            {
                slot = GetSlotOfItem(item.data);
                if (slot is null) break;
                slot.Add(item);

                OnSlotChanged?.Invoke(slot.id, slot.item);
            } while (item.amount > 0);

            if (slot is not null) return;

            Drop(item);
            #endregion
        }

        public void Drop(ItemStack item)
        {
            #region Drop
            // TODO
            #endregion
        }

        public ItemStack ClearSlot(int slotId)
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

        public bool AddToSlot(ItemStack item, int slotId)
        {
            #region AddToSlot
            if (slotId < 0 || slotId >= slots.Length)
            {
                Debug.LogWarning($"Tried to add to out-of-range slot {slotId}");
                return false;
            }
            Slot slot = slots[slotId];
            if (!slot.isEmpty && slot.item.data != item.data) return false;

            slot.Add(item);
            OnSlotChanged?.Invoke(slotId, slot.item);
            return true;
            #endregion
        }

        public bool RemoveAmount(int slotId, short amountToRemove)
        {
            #region RemoveAmount
            if (slotId < 0 || slotId >= slots.Length)
            {
                Debug.LogWarning($"Tried to add to out-of-range slot {slotId}");
                return false;
            }
            Slot slot = slots[slotId];
            if (slot.isEmpty || slot.item.amount < amountToRemove) return false;

            slot.item.amount -= amountToRemove;
            if (slot.item.amount == 0) ClearSlot(slotId);
            else OnSlotChanged(slotId, slot.item);
            return true;
            #endregion
        }

        private Slot GetSlotOfItem(ItemData itemData)
        {
            #region GetSlotOfItem
            foreach (Slot slot in slots)
            {
                if (!slot.isEmpty && slot.item.data == itemData && !slot.isFull) return slot;
            }
            foreach (Slot slot in slots)
            {
                if (slot.isEmpty || (slot.item.data == itemData && !slot.isFull)) return slot;
            }
            return null;
            #endregion
        }

        /// <summary>Method</summary>
        public void RemoveFromHand()
        {
            #region RemoveFromHand
            RemoveAmount(handIndex, 1);
            #endregion
        }
        #endregion
    }
}
