
using System;
using System.Collections.Generic;
using Data;
using Items;
using Player;
using Unity.VisualScripting;
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
        private IInventory inventory;
        public bool orphanAfterPickup = false;
        public static Item currentDraggedItem = null;
        #endregion

        #region Backers
        private ItemData _item;
        private Slot _slot;
        private bool isBeingDragged = false;
        private bool isGhost = false;
        #endregion

        #region Elements
        private VisualElement rootElm;
        private Image iconElm;
        private Label amountElm;

        public bool showingName = false;
        #endregion

        #region Constructor
        public Item() { Init(); }
        public Item(IInventory inventory, bool isDraggable, bool isGhost = false)
        {
            this.inventory = inventory;
            this.isDraggable = isDraggable;
            this.isGhost = isGhost;
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

            rootElm.RegisterCallback<PointerEnterEvent>(OnPointerEnter);
            rootElm.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
            if (isGhost)
            {
                UIController.Singleton.OnOverlayClose -= OnOverlayClose;
                UIController.Singleton.OnOverlayClose += OnOverlayClose;
            }

            // rootElm.RegisterCallback<PointerUpEvent>(OnPointerUp);
            #endregion
        }


        private void GrabItem(PointerDownEvent e)
        {

            Slot ghostSlot = orphanAfterPickup ? null : slot;
            IInventory ghostInventory = orphanAfterPickup ? Items.Inventory.Singleton : inventory;
            if (e.button == 1)
            {
                short stay, leave;
                stay = (short)Mathf.Floor(amount / 2);
                leave = (short)(amount - stay);

                Item ghost = new Item(ghostInventory, true, true)
                {
                    item = item,
                    amount = leave,
                    slot = ghostSlot,
                };

                ghost.style.position = Position.Absolute;
                ghost.style.left = e.position.x - _dragOffset.x;
                ghost.style.top = e.position.y - _dragOffset.y;
                panel.visualTree.Add(ghost);

                ghost.rootElm.AddToClassList("item-dragged");
                ghost.isBeingDragged = true;
                e.StopPropagation();
                // register as the currently dragged ghost and enable global pointer move
                currentDraggedItem = ghost;
                if (ghost.panel != null) ghost.panel.visualTree.RegisterCallback<PointerMoveEvent>(ghost.OnGlobalPointerMove);

                if (slot != null) inventory.RemoveAmount(slot.slotId, leave);
            }
            else
            {
                Item ghost = new Item(ghostInventory, true, true)
                {
                    item = item,
                    amount = amount,
                    slot = ghostSlot,
                };

                ghost.style.position = Position.Absolute;
                ghost.style.left = e.position.x - _dragOffset.x;
                ghost.style.top = e.position.y - _dragOffset.y;
                panel.visualTree.Add(ghost);

                ghost.rootElm.AddToClassList("item-dragged");
                ghost.isBeingDragged = true;
                e.StopPropagation();
                // register as the currently dragged ghost and enable global pointer move
                currentDraggedItem = ghost;
                if (ghost.panel != null) ghost.panel.visualTree.RegisterCallback<PointerMoveEvent>(ghost.OnGlobalPointerMove);

                if (slot != null) inventory.ClearSlot(slot.slotId);
            }
        }

        private void DropItem(PointerDownEvent e)
        {
            short dropAmount = e.button == 1 ? (short)1 : (short)amount;
            ItemStack stack = new(item) { amount = dropAmount };
            List<VisualElement> foundElements = new();
            panel.PickAll(e.position, foundElements);

            foreach (VisualElement element in foundElements)
            {
                if (element is not Slot targetSlot) continue;

                if (!targetSlot.isDroppable && targetSlot.item != null && targetSlot.item.item == item)
                {
                    ItemStack pulled = targetSlot.inventory.ClearSlot(targetSlot.slotId);
                    if (pulled != null) amount += pulled.amount;
                    return;
                }

                if (!targetSlot.isDroppable) break;
                if (e.button == 1 && targetSlot.item != null && targetSlot.item.item != item) return;



                // Try normal add first
                targetSlot.inventory.AddToSlot(stack, targetSlot.slotId);
                if (stack.amount == 0)
                {
                    amount -= dropAmount;
                    if (amount <= 0) Cleanup();
                    return;
                }
                if (stack.amount < dropAmount)
                {
                    amount -= dropAmount - stack.amount;
                    return;
                }

                // Failed — attempt swap (only on full left-click drop, not right-click single)
                if (e.button != 1 && targetSlot.inventory != null && targetSlot.item != null)
                {
                    ItemStack swapped = targetSlot.inventory.ClearSlot(targetSlot.slotId);
                    if (swapped != null)
                    {
                        targetSlot.inventory.AddToSlot(stack, targetSlot.slotId);
                        if (stack.amount == 0)
                        {
                            // Become the swapped item
                            item = swapped.data;
                            amount = swapped.amount;
                            return;
                        }
                        // AddToSlot still failed somehow — put it back
                        targetSlot.inventory.AddToSlot(swapped, targetSlot.slotId);
                    }
                }

                break;
            }

            if (e.button == 1) return;
            if (stack.amount == 0)
            {
                amount -= dropAmount;
                if (amount <= 0) Cleanup();
                return;
            }
            amount = stack.amount;
        }


        private void OnPointerDown(PointerDownEvent e)
        {
            #region OnPointerDown
            if (isBeingDragged || rootElm.HasPointerCapture(e.pointerId)) DropItem(e);
            else GrabItem(e);
            #endregion
        }
        private void OnPointerEnter(PointerEnterEvent e)
        {
            Vector2 pos = rootElm.worldBound.position;
            UIController.Singleton.ShowName(_item.name, pos + new Vector2(40, -10));
            showingName = true;
        }
        private void OnPointerLeave(PointerLeaveEvent e)
        {
            UIController.Singleton.HideName();
            showingName = false;
        }

        private void OnPointerMove(PointerMoveEvent e)
        {
            #region OnPointerMove

            if (!isBeingDragged) return;
            if (showingName)
            {
                UIController.Singleton.HideName();
                showingName = false;
            }

            Vector2 pos = e.position;
            style.left = pos.x - _dragOffset.x;
            style.top = pos.y - _dragOffset.y;
            #endregion
        }

        private void OnGlobalPointerMove(PointerMoveEvent e)
        {
            if (!isBeingDragged) return;
            Vector2 pos = e.position;
            style.left = pos.x - _dragOffset.x;
            style.top = pos.y - _dragOffset.y;
        }

        private void OnOverlayClose()
        {
            UIController.Singleton.OnOverlayClose -= OnOverlayClose;
            if (!isBeingDragged) return;

            ItemStack stack = new(item) { amount = (short)amount };
            if (slot == null || slot.inventory == null) Items.Inventory.Singleton.Add(stack);
            else slot.inventory.Add(stack, false);


            RemoveFromHierarchy();
        }

        private void Cleanup()
        {
            UIController.Singleton.OnOverlayClose -= OnOverlayClose;
            RemoveFromHierarchy();
        }
        #endregion
    }
}
