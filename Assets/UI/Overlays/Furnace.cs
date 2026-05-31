
using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
    [UxmlElement]
    public partial class Furnace : VisualElement
    {

        #region Data
        private Slot[] cookingSlots = new Slot[4];
        private Slot resultSlot;
        private Slot fuelSlot;

        private Slot[] allSlots = new Slot[Items.Overlays.Furnace.CookingSlots + 2];
        public Items.Overlays.Furnace furnace;
        #endregion

        #region Backers
        #endregion

        #region Elements
        private VisualElement rootElm;
        private VisualElement cookingHolder;
        private VisualElement cookingSlotsElm;
        private VisualElement fuelSlotHolder;
        private VisualElement cookingGrid;
        private VisualElement fill;
        private VisualElement fuelFill;
        #endregion

        #region Constructor
        public Furnace() { Init(); }
        public Furnace(Items.Overlays.Furnace furnace)
        {
            this.furnace = furnace;
            Init();
        }
        #endregion

        #region Methods
        private void Init()
        {
            #region Init
            VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI/Overlays/Furnace/Furnace");
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
            cookingHolder = this.Q<VisualElement>("cooking-holder");
            cookingSlotsElm = this.Q<VisualElement>("cooking-slots");
            cookingGrid = this.Q<VisualElement>("cooking-grid");
            fuelSlotHolder = this.Q<VisualElement>("fuel");
            fill = this.Q<VisualElement>("progress-fill");
            fuelFill = this.Q<VisualElement>("fuel-progress-fill");
            #endregion
        }

        private void CreateSlots()
        {
            #region CreateSlots

            for (short i = 0; i < cookingSlots.Length; i++)
            {
                Slot slot = new Slot(furnace, true); // TODO, run validation before drop

                int col = i % 2;
                int row = i / 2;
                if (col < 1) slot.AddToClassList("spaced-right");
                if (row < 1) slot.AddToClassList("spaced-bottom");

                cookingGrid.Add(slot);
                cookingSlots[i] = slot;
                slot.slotId = i;
                allSlots[slot.slotId] = slot;
            }

            resultSlot = new Slot(furnace, false, true) { slotId = (short)cookingSlots.Length };
            fuelSlot = new Slot(furnace, true) { slotId = (short)(cookingSlots.Length + 1) };

            cookingSlotsElm.Add(resultSlot);
            fuelSlotHolder.Add(fuelSlot);

            allSlots[resultSlot.slotId] = resultSlot;
            allSlots[fuelSlot.slotId] = fuelSlot;
            #endregion
        }
        private void SubscribeEvents()
        {
            #region SubscribeEvents
            if (furnace == null) return;
            furnace.OnSlotChanged += OnSlotChanged;
            furnace.OnFillChanged += OnFillChanged;
            furnace.OnFuelFillChanged += OnFuelFillChanged;
            #endregion
        }

        private void OnSlotChanged(int slotId, ItemStack item)
        {
            #region OnSlotChange
            Item itemElm = null;
            if (item is not null)
            {
                bool isResultSlot = slotId == resultSlot.slotId;

                itemElm = new Item(furnace, true)
                {
                    item = item.data,
                    amount = item.amount,
                    orphanAfterPickup = isResultSlot
                };

            }
            allSlots[slotId].item = itemElm;

            #endregion
        }

        private void OnFillChanged(float fillPercent)
        {
            fill.style.width = Length.Percent(fillPercent * 100);
        }

        private void OnFuelFillChanged(float fillPercent)
        {
            fuelFill.style.width = Length.Percent(fillPercent * 100);
        }
        #endregion
    }
}
