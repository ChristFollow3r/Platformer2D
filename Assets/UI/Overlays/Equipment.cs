
using Items;
using Player;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
    [UxmlElement]
    public partial class Equipment : VisualElement
    {

        #region Data
        private Slot[] equipmentSlots = new Slot[Items.Overlays.Equipment.EquipmentSlots];
        private Slot[] craftingSlots = new Slot[4];
        private Slot resultSlot;

        private Slot[] allSlots = new Slot[Items.Overlays.Equipment.EquipmentSlots + Items.Overlays.Equipment.CraftingSlots + 1];
        public Items.Overlays.Equipment equipment;
        #endregion

        #region Backers
        #endregion

        #region Elements
        private VisualElement rootElm;
        private VisualElement equipmentList;
        private VisualElement craftingHolder;
        private VisualElement craftingGrid;
        #endregion

        #region Constructor
        public Equipment() { Init(); }
        public Equipment(Items.Overlays.Equipment equipment)
        {
            this.equipment = equipment;
            Init();
        }
        #endregion

        #region Methods
        private void Init()
        {
            #region Init
            VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI/Overlays/Equipment/Equipment");
            tree.CloneTree(this);

            GetElements();
            CreateSlots();
            SubscribeEvents();
            #endregion
        }

        private void GetElements()
        {
            #region GetElements
            rootElm = this.Q<VisualElement>("root");
            equipmentList = this.Q<VisualElement>("equipment");
            craftingHolder = this.Q<VisualElement>("crafting-holder");
            craftingGrid = this.Q<VisualElement>("crafting-grid");
            #endregion
        }

        private void CreateSlots()
        {
            #region CreateSlots
            for (short i = 0; i < equipmentSlots.Length; i++)
            {
                Slot slot = new Slot(equipment, true);

                equipmentList.Add(slot);
                equipmentSlots[i] = slot;
                slot.slotId = i;
                allSlots[slot.slotId] = slot;
            }

            for (short i = 0; i < craftingSlots.Length; i++)
            {
                Slot slot = new Slot(equipment, true); // TODO, run validation before drop

                int col = i % 2;
                int row = i / 2;
                if (col < 1) slot.AddToClassList("spaced-right");
                if (row < 1) slot.AddToClassList("spaced-bottom");

                craftingGrid.Add(slot);
                craftingSlots[i] = slot;
                slot.slotId = (short)(equipmentSlots.Length + i);
                allSlots[slot.slotId] = slot;
            }

            resultSlot = new Slot(equipment, false) { slotId = (short)(equipmentSlots.Length + craftingSlots.Length) };
            craftingHolder.Add(resultSlot);
            allSlots[resultSlot.slotId] = resultSlot;
            #endregion
        }
        private void SubscribeEvents()
        {
            #region SubscribeEvents
            if (equipment == null) return;
            equipment.OnSlotChanged += OnSlotChanged;
            this.Q<Button>("recipeBtn").clicked += () => UIController.Singleton.OpenBook();
            #endregion
        }

        private void OnSlotChanged(int slotId, ItemStack item)
        {
            #region OnSlotChange
            Item itemElm = null;
            if (item is not null)
            {
                bool isResultSlot = slotId == resultSlot.slotId;

                itemElm = new Item(equipment, true)
                {
                    item = item.data,
                    amount = item.amount,
                    duration = item.duration,
                    orphanAfterPickup = isResultSlot
                };

            }
            allSlots[slotId].item = itemElm;

            #endregion
        }
        #endregion
    }
}
