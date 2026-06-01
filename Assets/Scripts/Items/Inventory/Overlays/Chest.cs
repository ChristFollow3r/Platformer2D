using System;
using System.Linq;
using Data;
using Items.Utils;
using Player;
using UnityEngine;


namespace Items.Overlays
{


    [Serializable]
    public class Chest : Overlay, IInventory
    {
        #region Data
        private const float CellSize = 0.5f;
        public const short RowSlots = 6;
        public const short ColSlots = 5;
        public const short MaxSlotId = ColSlots * RowSlots;
        public Slot[] slots = new Slot[ColSlots * RowSlots];
        #endregion

        #region Events
        public event Action<int, ItemStack> OnSlotChanged;
        #endregion

        #region Contructor
        public Chest(ulong blockId) : base(blockId, OverlayType.Chest)
        {
            Init();
        }
        #endregion

        #region Methods
        private void Init()
        {
            #region Init
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new Slot() { id = i };
            }
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

            Slot slot = slots[slotId];

            if (!slot.isEmpty && slot.item.data != itemStack.data) return false;
            slot.Add(itemStack);
            OnSlotChanged?.Invoke(slotId, slot.item);
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
            Slot slot = slots[slotId];

            slot.item.amount -= amount;
            if (slot.item.amount <= 0) ClearSlot(slotId);
            else OnSlotChanged(slotId, slot.item);
            return true;
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



        public override void RefreshUI()
        {
            #region RefreshUI
            foreach (Slot slot in slots)
            {
                if (slot.isEmpty) continue;
                OnSlotChanged?.Invoke(slot.id, slot.item);
            }
            #endregion
        }

        public override void OnBlockDestroyed()
        {
            var (x, y) = BlockIdUtils.ToCell(blockId);
            Vector2 pos = new Vector2(x * CellSize, y * CellSize);

            foreach (Slot slot in slots)
            {
                if (!slot.isEmpty) Inventory.Singleton.Drop(slot.item, pos);
            }
        }

        public string ToJson()
        {
            CloseOverlay();
            return JsonUtility.ToJson(new ChestData
            {
                slots = slots.Where(s => !s.isEmpty).Select(s => new SlotData
                {
                    id = s.id,
                    itemId = s.isEmpty ? null : s.item.data.name,
                    amount = s.isEmpty ? (short)0 : s.item.amount,
                    duration = s.isEmpty ? 0f : s.item.duration
                }).ToArray()
            });
        }

        public void FromJson(string json)
        {
            ChestData data = JsonUtility.FromJson<ChestData>(json);
            if (data == null) return;
            ItemDatabase db = Resources.Load<ItemDatabase>("ItemDatabase");
            if (!db)
            {
                Debug.LogError("Item db not found!");
                return;
            }
            for (int i = 0; i < data.slots.Length && i < slots.Length; i++)
            {
                SlotData s = data.slots[i];
                if (s.itemId == null) continue;
                ItemData itemData = db.items.Find(item => item.name == s.itemId);

                slots[s.id].item = string.IsNullOrEmpty(s.itemId) ? null
                    : new ItemStack(itemData) { amount = s.amount };
                OnSlotChanged?.Invoke(s.id, slots[s.id].item);
            }
        }
        #endregion
    }
}
