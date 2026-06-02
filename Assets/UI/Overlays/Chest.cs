
using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
    [UxmlElement]
    public partial class Chest : VisualElement
    {

        #region Data
        private Slot[] slots = new Slot[Items.Overlays.Chest.MaxSlotId];
        public Items.Overlays.Chest chest;
        #endregion



        #region Elements
        private VisualElement rootElm;
        private VisualElement chestGrid;
        #endregion

        #region Constructor
        public Chest() { Init(); }
        public Chest(Items.Overlays.Chest chest)
        {
            this.chest = chest;
            Init();
        }
        #endregion

        #region Methods
        private void Init()
        {
            #region Init
            VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI/Overlays/Chest/Chest");
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
            chestGrid = this.Q<VisualElement>("grid");
            #endregion
        }

        private void CreateSlots()
        {
            #region CreateSlots

            for (short i = 0; i < slots.Length; i++)
            {
                Slot slot = new Slot(chest, true); // TODO, run validation before drop

                int col = i % Items.Overlays.Chest.ColSlots;
                int row = i / Items.Overlays.Chest.ColSlots;

                if (col < Items.Overlays.Chest.ColSlots - 1) slot.AddToClassList("spaced-right");
                if (row < Items.Overlays.Chest.RowSlots - 1) slot.AddToClassList("spaced-bottom");

                chestGrid.Add(slot);
                slots[i] = slot;
                slot.slotId = i;
            }
            #endregion
        }
        private void SubscribeEvents()
        {
            #region SubscribeEvents
            if (chest == null) return;
            chest.OnSlotChanged += OnSlotChanged;
            #endregion
        }

        private void OnSlotChanged(int slotId, ItemStack item)
        {
            #region OnSlotChange
            Item itemElm = null;
            if (item is not null)
            {
                itemElm = new Item(chest, true)
                {
                    item = item.data,
                    amount = item.amount,
                    duration = item.duration
                };
            }
            slots[slotId].item = itemElm;

            #endregion
        }
        #endregion
    }
}
