
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
  [UxmlElement]
  public partial class Slot : VisualElement
  {
    #region Data
    [UxmlAttribute] public Item item { get => _item; set => SetItem(value); }

    public bool hasItem => item != null;
    #endregion

    #region Backers
    private Item _item;
    #endregion

    #region Elements
    private VisualElement rootElm;
    private VisualElement itemHolderElm;
    #endregion

    #region Constructor
    public Slot()
    {
      VisualTreeAsset tree = UnityEngine.Resources.Load<VisualTreeAsset>("UI/Components/Slot/Slot");
      tree.CloneTree(this);

      GetElements();
    }
    #endregion

    #region Setters
    private void SetItem(Item item)
    {
      #region SetItem
      _item = item;
      if (itemHolderElm.childCount != 0) itemHolderElm.RemoveAt(0);
      itemHolderElm.Add(item);
      item.slot = this;
      #endregion
    }
    #endregion

    #region Methods
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
