
using UnityEngine.UIElements;

namespace UI.Components
{
    [UxmlElement]
    public partial class Item : VisualElement
    {

        #region Data
        [UxmlAttribute] public Scriptable_Objects_Scripts.Item item { get => _item; set => SetItem(value); }
        [UxmlAttribute] public int amount { get => int.Parse(amountElm.text); set => amountElm.text = value.ToString(); }
        #endregion

        #region Backers
        private Scriptable_Objects_Scripts.Item _item;
        #endregion

        #region Elements
        private VisualElement rootElm;
        private Image iconElm;
        private Label amountElm;
        #endregion

        #region Constructor
        public Item()
        {
            VisualTreeAsset tree = UnityEngine.Resources.Load<VisualTreeAsset>("UI/Components/Slot/Slot");
            tree.CloneTree(this);

            GetElements();
        }
        #endregion

        #region Setters
        private void SetItem(Scriptable_Objects_Scripts.Item item)
        {
            #region SetItem
            _item = item;
            iconElm.image = _item.itemIcon;
            #endregion
        }
        #endregion

        #region Methods
        private void GetElements()
        {
            #region GetElements
            rootElm = this.Q<VisualElement>("root");
            iconElm = this.Q<Image>("icon");
            amountElm = this.Q<Label>("amount");
            #endregion
        }
        #endregion
    }
}