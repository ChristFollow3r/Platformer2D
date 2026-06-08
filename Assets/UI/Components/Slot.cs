
using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
    [UxmlElement]
    public partial class Slot : VisualElement
    {
        #region Data
        [UxmlAttribute] public Item item { get => _item; set => SetItem(value); }
        public short slotId;
        public bool hasItem => item != null;

        public bool isDroppable;
        public bool isStatic;
        public IInventory inventory;
        #endregion

        #region Backers
        private Item _item;
        #endregion

        #region Elements
        private VisualElement rootElm;
        public VisualElement itemHolderElm;
        #endregion

        #region Constructor
        public Slot() { Init(); }
        public Slot(IInventory inventory, bool isDroppable, bool isStatic = false)
        {
            this.inventory = inventory;
            this.isDroppable = isDroppable;
            this.isStatic = isStatic;
            Init();
        }
        #endregion

        #region Setters
        private void SetItem(Item item)
        {
            #region SetItem
            _item = item;
            if (itemHolderElm.childCount != 0) itemHolderElm.RemoveAt(0);
            if (item == null) return;
            itemHolderElm.Add(item);
            item.slot = this;
            #endregion
        }
        #endregion

        #region Methods
        /// <summary>Method</summary>
        private void Init()
        {
            #region Init
            VisualTreeAsset tree = UnityEngine.Resources.Load<VisualTreeAsset>("UI/Components/Slot/Slot");
            tree.CloneTree(this);

            GetElements();
            #endregion
        }
        private void GetElements()
        {
            #region GetElements
            rootElm = this.Q<VisualElement>("root");
            itemHolderElm = this.Q<VisualElement>("item-holder");
            if (itemHolderElm is null) Debug.LogError("Can't get the holder dingy");
            #endregion
        }
        #endregion
    }
}
