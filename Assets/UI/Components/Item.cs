
using System;
using System.Collections.Generic;
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

    private Vector2 _dragOffset = new Vector2(50, 50);
    private bool isDraggable;
    #endregion

    #region Backers
    private ItemData _item;
    private Slot _slot;
    private bool isBeingDragged = false;
    #endregion

    #region Elements
    private VisualElement rootElm;
    private Image iconElm;
    private Label amountElm;
    #endregion

    #region Constructor
    public Item() { Init(); }
    public Item(bool isDraggable)
    {
      this.isDraggable = isDraggable;
      Init();
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
    #endregion

    #region Methods
    private void Init()
    {
      #region Init
      VisualTreeAsset tree = Resources.Load<VisualTreeAsset>("UI/Components/Item/Item");
      tree.CloneTree(this);

      GetElements();
      SubscribeEvents();
      #endregion
    }

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
      if (!isDraggable) return;
      rootElm.RegisterCallback<PointerDownEvent>(OnPointerDown);
      rootElm.RegisterCallback<PointerMoveEvent>(OnPointerMove);
      rootElm.RegisterCallback<PointerUpEvent>(OnPointerUp);
      #endregion
    }


    private void OnPointerDown(PointerDownEvent e)
    {
      #region OnPointerDown

      if (e.button == 1)
      {
        short stay, leave;
        stay = (short)Mathf.Floor(amount / 2);
        leave = (short)(amount - stay);

        Item ghost = new Item(true)
        {
          item = item,
          amount = leave,
          slot = slot
        };

        ghost.style.position = Position.Absolute;
        ghost.style.left = e.position.x - _dragOffset.x;
        ghost.style.top = e.position.y - _dragOffset.y;
        panel.visualTree.Add(ghost);

        ghost.rootElm.CapturePointer(e.pointerId);
        ghost.rootElm.AddToClassList("item-dragged");
        ghost.isBeingDragged = true;
        e.StopPropagation();

        Items.Inventory.RemoveAmount(slot.slotId, leave);
      }
      else
      {
        Item ghost = new Item(true)
        {
          item = item,
          amount = amount,
          slot = slot
        };

        ghost.style.position = Position.Absolute;
        ghost.style.left = e.position.x - _dragOffset.x;
        ghost.style.top = e.position.y - _dragOffset.y;
        panel.visualTree.Add(ghost);

        ghost.rootElm.CapturePointer(e.pointerId);
        ghost.rootElm.AddToClassList("item-dragged");
        ghost.isBeingDragged = true;
        e.StopPropagation();

        Items.Inventory.ClearSlot(slot.slotId);
      }

      #endregion
    }

    private void OnPointerMove(PointerMoveEvent e)
    {
      #region OnPointerMove
      if (!isBeingDragged || !rootElm.HasPointerCapture(e.pointerId)) return;

      CancelMove();

      Vector2 pos = e.position;
      style.left = pos.x - _dragOffset.x;
      style.top = pos.y - _dragOffset.y;
      #endregion
    }

    private void OnPointerUp(PointerUpEvent e)
    {
      #region OnPointerUp
      if (e.button == 1) return; // TODO: leave 1
      if (!isBeingDragged || !rootElm.HasPointerCapture(e.pointerId)) return;
      rootElm.ReleasePointer(e.pointerId);
      rootElm.RemoveFromClassList("item-dragged");

      Items.ItemStack stack = new() { data = item, amount = (short)amount, };
      List<VisualElement> foundElements = new();
      panel.PickAll(e.position, foundElements);

      foreach (VisualElement element in foundElements)
      {
        // TODO: Add drag to outside
        if (element is not Slot targetSlot) continue;

        bool sucess = Items.Inventory.AddToSlot(stack, targetSlot.slotId);
        if (sucess) { RemoveFromHierarchy(); return; }
        break;
      }

      if (!Items.Inventory.AddToSlot(stack, slot.slotId)) Items.Inventory.Add(stack);
      RemoveFromHierarchy();
      #endregion
    }
    #endregion

    #region Utils
    private IVisualElementScheduledItem moveTween;

    private void MoveTo(Vector2 target, float duration)
    {
      #region MoveTo
      CancelMove();

      float elapsed = 0f;
      float startX = style.left.value.value;
      float startY = style.top.value.value;

      moveTween = schedule.Execute(() =>
      {
        elapsed += 16;
        float t = Mathf.Clamp01(elapsed / duration);
        float ease = t * t * t; // ease in cubic

        style.left = Mathf.Lerp(startX, target.x, ease);
        style.top = Mathf.Lerp(startY, target.y, ease);

        if (t >= 1f) CancelMove();
      }).Every(16);
      #endregion
    }

    private void CancelMove()
    {
      #region CancelMove
      moveTween?.Pause();
      moveTween = null;
      #endregion
    }
    #endregion
  }
}
