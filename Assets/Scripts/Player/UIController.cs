using System;
using System.Collections.Generic;
using Items;
using UI.Components;
using UnityEngine;
using UnityEngine.UIElements;


namespace Player
{
    public enum OverlayType
    {
        Inventory,
        Furnace
    };

    [DefaultExecutionOrder(-50)]
    public class UIController : MonoBehaviour
    {
        #region Singleton setup
        public static UIController Singleton;
        private void SetupSingleton()
        {
            #region SetupSingleton
            if (Singleton != null && Singleton != this) { Destroy(gameObject); return; }
            Singleton = this;
            #endregion
        }
        #endregion


        #region Data
        [Header("Elements")]
        [SerializeField] private UIDocument overlay;
        [SerializeField] private UIDocument hud;
        private VisualElement overlayRoot;
        private VisualElement hudRoot;

        private Dictionary<ulong, Overlay> overlaysByBlockId = new();

        [Header("Controls")]
        [SerializeField] private bool isOverlayOpen;
        private InputSystem_Actions playerInput;
        #endregion

        #region Events
        public event Action<ulong, OverlayType> OnOverlayOpen;
        public event Action OnOverlayClose;
        #endregion

        #region Unity
        /// <summary>Ran by unity on load</summary>
        private void Awake()
        {
            #region Awake
            SetupSingleton();
            playerInput = new InputSystem_Actions();
            playerInput.Enable();
            CreateUI();
            #endregion
        }
        /// <summary>Ran by unity on first enable</summary>
        private void Start()
        {
            #region Start
            CloseOverlay();
            #endregion
        }
        /// <summary>Ran by unity each frame</summary>
        private void Update()
        {
            #region Update
            CheckOverlay();
            MoveHand();
            #endregion
        }
        #endregion

        #region Methods
        private void CreateUI()
        {
            #region CreateUI
            UI.Components.Inventory inventory = new UI.Components.Inventory();
            Hotbar overlayHotbar = new Hotbar { isMain = false };

            overlay.rootVisualElement.Q("inventory").Add(inventory);
            overlay.rootVisualElement.Q("holder").Add(overlayHotbar);

            Hotbar hudHotbar = new Hotbar { isMain = true };
            hud.rootVisualElement.Q("hotbar-holder").Add(hudHotbar);
            CreateOverlay(ulong.MinValue, OverlayType.Inventory);
            #endregion
        }

        private void CheckOverlay()
        {
            #region CheckOverlay
            if (playerInput.UI.OpenInventory.WasPressedThisFrame())
            {
                isOverlayOpen = !isOverlayOpen;

                if (isOverlayOpen) OpenOverlay(ulong.MinValue);
                else CloseOverlay();
                return;
            }

            if (playerInput.UI.CloseOverlay.WasPressedThisFrame() && isOverlayOpen) CloseOverlay();
            #endregion
        }

        public void OpenOverlay(ulong blockId)
        {
            #region OpenOverlay
            hud.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.None;
            overlay.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.Flex;
            isOverlayOpen = true;

            if (!overlaysByBlockId.TryGetValue(blockId, out Overlay foundOverlay)) return;

            VisualElement overlayElement = CreateElement(foundOverlay);
            VisualElement left = overlay.rootVisualElement.Q("left");
            if (left.childCount != 0) left.RemoveAt(0);
            left.Add(overlayElement);

            OnOverlayOpen?.Invoke(blockId, foundOverlay.overlayType);
            foundOverlay.RefreshUI();
            #endregion
        }

        public void CloseOverlay()
        {
            #region CloseOverlay
            OnOverlayClose?.Invoke();
            hud.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.Flex;
            overlay.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.None;
            isOverlayOpen = false;
            #endregion
        }

        private VisualElement CreateElement(Overlay foundOverlay)
        {
            #region CreateElement
            switch (foundOverlay.overlayType)
            {
                case OverlayType.Inventory:
                    {
                        Items.Overlays.Equipment data = (Items.Overlays.Equipment)foundOverlay;
                        VisualElement element = new Equipment(data);
                        return element;
                    }

                case OverlayType.Furnace:
                    {
                        Items.Overlays.Furnace data = (Items.Overlays.Furnace)foundOverlay;
                        VisualElement element = new Furnace(data);
                        return element;
                    }
                default:
                    return null;
            }
            #endregion
        }

        public void DestroyEntity(ulong blockId)
        {
            #region DestroyEntity
            overlaysByBlockId.Remove(blockId);
            #endregion
        }

        public Overlay CreateOverlay(ulong blockId, OverlayType overlayType)
        {
            #region CreateOverlay
            switch (overlayType)
            {
                case OverlayType.Inventory:
                    {
                        Items.Overlays.Equipment data = new Items.Overlays.Equipment();
                        overlaysByBlockId[blockId] = data;
                        return data;
                    }

                case OverlayType.Furnace:
                    {
                        Items.Overlays.Furnace data = new Items.Overlays.Furnace(blockId);
                        overlaysByBlockId[blockId] = data;
                        return data;
                    }
                default:
                    return null;
            }
            #endregion
        }

        private void MoveHand()
        {
            #region MoveHand
            Vector2 move = playerInput.UI.MoveHand.ReadValue<Vector2>();
            if (move.y == 0) return;

            if (move.y > 0) Items.Inventory.Singleton.handIndex += 1;
            else Items.Inventory.Singleton.handIndex -= 1;
            #endregion
        }
        #endregion
    }
}
