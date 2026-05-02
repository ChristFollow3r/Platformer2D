
using Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Components
{
  [UxmlElement]
  public partial class Item : VisualElement
  {

    #region Data
    [UxmlAttribute] public ItemData item { get => _item; set => SetItem(value); }
    [UxmlAttribute] public Slot slot { get => _slot; set => _slot = value; }
    [UxmlAttribute] public int amount { get => int.Parse(amountElm.text); set => amountElm.text = value.ToString(); }
    [UxmlAttribute] public bool isBeingDragged { get => _isBeingDragged; set => SetIsBeingDragged(value); }

    private Vector2 _dragOffset;
    #endregion

    #region Backers
    private ItemData _item;
    private Slot _slot;
    private bool _isBeingDragged = false;
    #endregion

    #region Elements
    private VisualElement rootElm;
    private Image iconElm;
    private Label amountElm;
    #endregion

    #region Constructor
    public Item()
    {
      VisualTreeAsset tree = UnityEngine.Resources.Load<VisualTreeAsset>("UI/Components/Item/Item");
      tree.CloneTree(this);

      GetElements();
      SubscribeEvents();
    }
    #endregion

    #region Setters
    private void SetItem(ItemData item)
    {
      #region SetItem
      _item = item;
      iconElm.image = _item.sprite.texture;
      #endregion
    }

    private void SetIsBeingDragged(bool isBeingDragged)
    {
      #region SetIsBeingDragged
      _isBeingDragged = isBeingDragged;
      if (_isBeingDragged) AddToClassList("item-dragged");
      else RemoveFromClassList("item-dragged");
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

    private void SubscribeEvents()
    {
      #region SubscribeEvents
      Debug.Log($"rootElm is null: {rootElm == null}");
      Debug.Log($"rootElm pickingMode: {rootElm?.pickingMode}");

      rootElm.RegisterCallback<PointerDownEvent>(OnPointerDown);
      rootElm.RegisterCallback<PointerMoveEvent>(OnPointerMove);
      rootElm.RegisterCallback<PointerUpEvent>(OnPointerUp);
      #endregion
    }


    private void OnPointerDown(PointerDownEvent e)
    {
      #region OnPointerDown
      var panelRoot = panel.visualTree.Q<VisualElement>();
      panelRoot.Add(this);

      isBeingDragged = true;
      _dragOffset = e.localPosition;
      rootElm.CapturePointer(e.pointerId);
      e.StopPropagation();
      #endregion
    }

    private void OnPointerMove(PointerMoveEvent e)
    {
      #region OnPointerMove
      if (!isBeingDragged || !rootElm.HasPointerCapture(e.pointerId)) return;
      Vector2 localPos = parent.WorldToLocal(e.position);
      style.left = localPos.x - _dragOffset.x;
      style.top = localPos.y - _dragOffset.y;
      #endregion
    }

    private void OnPointerUp(PointerUpEvent e)
    {
      #region OnPointerUp
      isBeingDragged = false;
      rootElm.ReleasePointer(e.pointerId);

      RemoveFromHierarchy();
      slot.item = this;

      style.left = 0;
      style.top = 0;
      #endregion
    }
    #endregion
  }
}
