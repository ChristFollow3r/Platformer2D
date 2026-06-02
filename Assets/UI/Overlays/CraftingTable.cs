
using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
    [UxmlElement]
    public partial class CraftingTable : VisualElement
    {

        #region Data
        private Slot[] craftingSlots = new Slot[16];
        private Slot resultSlot;
        private Slot[] allSlots = new Slot[Items.Overlays.CraftingTable.MaxSlotId + 1];
        public Items.Overlays.CraftingTable craftingTable;
        #endregion

        #region Backers
        #endregion

        #region Elements
        private VisualElement rootElm;
        private VisualElement craftingHolder;
        private VisualElement craftingGrid;
        #endregion

        #region Constructor
        public CraftingTable() { Init(); }
        public CraftingTable(Items.Overlays.CraftingTable craftingTable)
        {
            this.craftingTable = craftingTable;
            Init();
        }
        #endregion

        #region Methods
        private void Init()
        {
            #region Init
            VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI/Overlays/CraftingTable/CraftingTable");
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
            craftingHolder = this.Q<VisualElement>("crafting-holder");
            craftingGrid = this.Q<VisualElement>("crafting-grid");
            #endregion
        }

        private void CreateSlots()
        {
            #region CreateSlots

            for (short i = 0; i < craftingSlots.Length; i++)
            {
                Slot slot = new Slot(craftingTable, true); // TODO, run validation before drop

                int col = i % 4;
                int row = i / 4;
                if (col < 3) slot.AddToClassList("spaced-right");
                if (row < 3) slot.AddToClassList("spaced-bottom");

                craftingGrid.Add(slot);
                craftingSlots[i] = slot;
                slot.slotId = i;
                allSlots[slot.slotId] = slot;
            }

            resultSlot = new Slot(craftingTable, false) { slotId = (short)craftingSlots.Length };
            craftingHolder.Add(resultSlot);
            allSlots[resultSlot.slotId] = resultSlot;
            #endregion
        }
        private void SubscribeEvents()
        {
            #region SubscribeEvents
            if (craftingTable == null) return;
            craftingTable.OnSlotChanged += OnSlotChanged;
            #endregion
        }

        private void OnSlotChanged(int slotId, ItemStack item)
        {
            #region OnSlotChange
            Item itemElm = null;
            if (item is not null)
            {
                bool isResultSlot = slotId == resultSlot.slotId;

                itemElm = new Item(craftingTable, true)
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
