using System;
using System.Linq;
using Data;
using Items.Utils;
using Player;
using UnityEngine;


namespace Items.Overlays
{

    public enum EquipmentType
    {
        Mod,
        NONE,
    }

    public enum Mod
    {
        Stone,
        Copper,
        Bronze,
        Iron,
        Primordial,
        NONE
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

        private RuntimeAnimatorController defaultController;
        bool modQueued = false;
        #endregion

        #region Contructor
        public Equipment(RuntimeAnimatorController defaultController) : base(ulong.MinValue, Player.OverlayType.Inventory)
        {
            this.defaultController = defaultController;
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
            if (mod == null)
            {

                UIController.Singleton.UpdateMod(0, 1);
                return;
            }
            else
            {
                if (modQueued)
                {

                    GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
                    if (playerGO)
                    {
                        playerGO.GetComponent<Animator>().runtimeAnimatorController = mod.data.modData.controller;
                        modQueued = false;
                    }
                }
            }

            mod.duration -= Time.deltaTime;
            UIController.Singleton.UpdateMod(mod.duration, mod.data.modData.duration);
            if (mod.duration >= 0) return;
            ClearSlot((int)EquipmentType.Mod);

            #endregion
        }

        public bool AddEquipment(EquipmentType equipmentType, ItemStack itemStack)
        {
            #region AddEquipment




            if (equipmentType == EquipmentType.Mod)
            {
                Slot slot = equipmentSlots[(int)equipmentType];
                slot.Add(itemStack);
                OnSlotChanged?.Invoke(slot.id, slot.item);

                OnModChange?.Invoke(true, itemStack.data.modData.mod);
                GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
                if (!playerGo) return true;
                playerGo.GetComponent<Animator>().runtimeAnimatorController = itemStack.data.modData.controller;
            }
            else return false;
            return true;
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

        public void Add(ItemStack itemStack, bool stacked = true) => Inventory.Singleton.Drop(itemStack);

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
                return AddEquipment(itemStack.data.equipmentType, itemStack);
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
            Debug.Log($"Attepting to clear slot {slotId}");
            Slot slot;
            bool isCraftingSlot = false;
            bool isResultSlot = slotId == resultSlot.id;



            if (isResultSlot) slot = resultSlot;
            else
            {
                isCraftingSlot = slotId >= EquipmentSlots && slotId < EquipmentSlots + CraftingSlots;
                slot = isCraftingSlot ? craftingSlots[slotId - EquipmentSlots] : equipmentSlots[slotId];
            }

            if (slot.isEmpty)
            {
                Debug.Log($"Slot is empty");
                return null;
            }

            ItemStack itemStack = slot.item;
            slot.item = null;

            OnSlotChanged?.Invoke(slotId, null);
            Debug.Log($"Slot cleared!");

            if (isCraftingSlot || isResultSlot) EvaluateCraft();
            if (isResultSlot)
            {
                for (int i = 0; i < CraftingSlots; i++)
                {
                    if (craftingSlots[i].isEmpty) continue;
                    RemoveAmount(craftingSlots[i].id, 1);
                }
            }
            if (slot.id == (int)EquipmentType.Mod)
            {
                GameObject playerGo = GameObject.FindGameObjectWithTag("Player");
                if (playerGo)
                {
                    playerGo.GetComponent<Animator>().runtimeAnimatorController = defaultController;
                    OnModChange?.Invoke(false, 0);
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

        public float GetHitPower()
        {
            ItemStack modItem = equipmentSlots[(int)EquipmentType.Mod].item;
            if (modItem == null) return 1f;
            if (modItem.data.modData == null) return 1f;
            return modItem.data.modData.attackPower;
        }

        public int GetDefence()
        {
            ItemStack modItem = equipmentSlots[(int)EquipmentType.Mod].item;
            if (modItem == null) return 0;
            if (modItem.data.modData == null) return 0;
            return modItem.data.modData.defence;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(new EquipmentData
            {
                equipmentSlots = equipmentSlots.Where(s => !s.isEmpty).Select(s => new SlotData(s)).ToArray()
            });
        }

        public void FromJson(string json)
        {
            EquipmentData data = JsonUtility.FromJson<EquipmentData>(json);
            if (data == null) return;

            ItemDatabase db = Resources.Load<ItemDatabase>("ItemDatabase");

            for (int i = 0; i < data.equipmentSlots.Length && i < equipmentSlots.Length; i++)
            {
                SlotData s = data.equipmentSlots[i];
                equipmentSlots[s.id].item = string.IsNullOrEmpty(s.itemId) ? null
                    : new ItemStack(db.items.Find(item => item.name == s.itemId)) { amount = s.amount, duration = s.duration };
                OnSlotChanged?.Invoke(s.id, equipmentSlots[s.id].item);

                if (s.id == (int)EquipmentType.Mod && !equipmentSlots[s.id].isEmpty)
                {
                    modQueued = true;
                }
            }
        }
        #endregion
    }
}
