using System.Collections.Generic;
using Data;
using Player;
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

        [Header("Prefabs")][SerializeField] private GameObject itemEntityPrefab;
        public short handIndex
        {
            get => _handIndex; set
            {
                _handIndex = value;
                if (_handIndex == HotbarSlots) _handIndex = 0;
                if (_handIndex < 0) _handIndex = HotbarSlots - 1;

                OnHandChanged?.Invoke(_handIndex);
                string name = hand == null ? "" : hand.data.name;
                UIController.Singleton.SetItemName(name);
            }
        }
        private short _handIndex = 0;
        private bool isInitialized = false;

        [SerializeField] private List<ItemStackBuilder> startingItems = new();
        [SerializeField] private AudioClip pickupSound;
        #endregion

        #region Events
        public event System.Action<int, ItemStack> OnSlotChanged;
        public event System.Action<short> OnHandChanged;
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
            foreach (ItemStackBuilder builder in startingItems)
            {
                ItemStack itemStack = new ItemStack(builder.data) { amount = builder.amount };
                Add(itemStack);
            }

            isInitialized = true;
            #endregion
        }

        private void OnDestroy()
        {
            #region OnDestroy

            if (Singleton == this)
            {
                Singleton = null;
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

        public bool Fits(ItemData item) => _Add(new(item) { amount = 1 }, true);

        public void Add(ItemStack item, bool stacked = true) => _Add(item, false, stacked);

        private bool _Add(ItemStack item, bool dryRun = false, bool stacked = true)
        {
            if (item == null || item.data == null) return false;
            Slot slot;
            bool itemWasAdded = false;

            do
            {
                slot = GetSlotOfItem(item.data, stacked);
                if (slot is null) break;
                if (!dryRun)
                {
                    slot.Add(item);
                    itemWasAdded = true;
                    OnSlotChanged?.Invoke(slot.id, slot.item);
                }
                else break;
            } while (item.amount > 0);

            if (!dryRun && itemWasAdded) PlayPickupSound();

            if (!dryRun && item.amount > 0) Drop(item);
            return slot is not null;
        }

        public void Drop(ItemStack item)
        {
            #region Drop
            if (item == null || item.data == null) return;

            if (PlayerMovement.Singleton == null) return;
            Drop(item, PlayerMovement.Singleton.gameObject.transform.position);
            #endregion
        }

        public void Drop(ItemStack item, Vector2 pos)
        {
            #region Drop
            if (item == null || item.data == null) return;

            GameObject droppedItem = Instantiate(itemEntityPrefab, pos, Quaternion.identity);
            Vector2 randomOffset = new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f));
            droppedItem.transform.position += (Vector3)randomOffset;

            if (droppedItem.TryGetComponent(out ItemEntity entity))
            {
                entity.Initialize(item);
            }

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
            PlayPickupSound();

            OnSlotChanged?.Invoke(slotId, slot.item);
            return item.amount == 0;
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

        private Slot GetSlotOfItem(ItemData itemData, bool stacked)
        {
            #region GetSlotOfItem
            if (!stacked)
            {
                foreach (Slot slot in slots)
                {
                    if (slot.isEmpty) return slot;
                }
                return null;
            }

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

        public void RemoveFromHand()
        {
            #region RemoveFromHand
            RemoveAmount(handIndex, 1);
            #endregion
        }


        private void PlayPickupSound()
        {
            if (pickupSound != null && isInitialized)
            {
                // Plays the sound directly at the Main Camera's position
                if (Camera.main != null)
                {
                    AudioSource.PlayClipAtPoint(pickupSound, Camera.main.transform.position);
                }
                else
                {
                    Debug.LogWarning("Camera.main is null! Make sure your camera has the 'MainCamera' tag.");
                }
            }
        }

        public string ToJson()
        {
            var data = new InventoryData
            {
                slots = new SlotData[slots.Length]
            };

            for (int i = 0; i < slots.Length; i++)
            {
                Slot slot = slots[i];
                if (slot.isEmpty) continue;

                data.slots[i] = new SlotData(slot);
            }

            return JsonUtility.ToJson(data);

        }
        public void FromJson(string json)
        {
            InventoryData data = JsonUtility.FromJson<InventoryData>(json);
            if (data == null) return;

            ItemDatabase db = Resources.Load<ItemDatabase>("ItemDatabase");

            for (int i = 0; i < data.slots.Length && i < slots.Length; i++)
            {
                SlotData slotData = data.slots[i];

                if (string.IsNullOrEmpty(slotData.itemId) || slotData.amount <= 0)
                {
                    slots[i].item = null;
                }
                else
                {
                    ItemData itemData = db.items.Find(item => item.name == slotData.itemId);
                    if (itemData != null)
                        slots[slotData.id].item = new ItemStack(itemData) { amount = slotData.amount, duration = slotData.duration };
                    else
                        Debug.LogWarning($"[Inventory] Unknown item id '{slotData.itemId}' in slot {i}");
                }

                OnSlotChanged?.Invoke(slotData.id, slots[slotData.id].item);
            }

            foreach (ItemStackBuilder builder in startingItems)
            {
                ItemStack itemStack = new ItemStack(builder.data) { amount = builder.amount, };
                Add(itemStack);
            }
        }
        #endregion
    }
}
