
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

      isBeingDragged = true;
      rootElm.CapturePointer(e.pointerId);
      e.StopPropagation();

      Vector2 localPos = parent.WorldToLocal(e.position);
      Vector2 target = localPos - _dragOffset;
      TweenTo(target, 100);

      rootElm.AddToClassList("item-dragged");
      #endregion
    }

    private void OnPointerMove(PointerMoveEvent e)
    {
      #region OnPointerMove
      if (!isBeingDragged || !rootElm.HasPointerCapture(e.pointerId)) return;

      StopTween();

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


      TweenTo(Vector2.zero, 100);
      rootElm.RemoveFromClassList("item-dragged");
      #endregion
    }
    #endregion

    #region Utils
    private IVisualElementScheduledItem _positionTween;

    private void TweenTo(Vector2 target, float duration)
    {
      StopTween();

      float elapsed = 0f;
      float startX = style.left.value.value;
      float startY = style.top.value.value;

      _positionTween = schedule.Execute(() =>
      {
        elapsed += 16;
        float t = Mathf.Clamp01(elapsed / duration);
        float ease = t * t * t; // ease in cubic

        style.left = Mathf.Lerp(startX, target.x, ease);
        style.top = Mathf.Lerp(startY, target.y, ease);

        if (t >= 1f) StopTween();
      }).Every(16);
    }

    private void StopTween()
    {
      _positionTween?.Pause();
      _positionTween = null;
    }
    #endregion
  }
}
