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
    Inventory
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
    [SerializeField] private GameObject uiHolder;
    [SerializeField] private UIDocument overlay;
    [SerializeField] private UIDocument hud;
    private VisualElement overlayRoot;
    private VisualElement hudRoot;

    private Items.Overlays.Equipment equipmentOverlay;
    private Dictionary<int, Overlay> overlaysByBlockId;

    [Header("Controls")]
    [SerializeField] private bool isOverlayOpen;
    private InputSystem_Actions playerInput;
    #endregion

    #region Events
    public event Action<int, OverlayType, object> OnOverlayOpen;
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
      #endregion
    }
    #endregion

    #region Methods
    /// <summary>Method</summary>
    private void CreateUI()
    {
      #region CreateUI
      // Create Overlay invent+hotbar
      Debug.Log($"Singleton is valid? {Items.Inventory.Singleton != null}");
      UI.Components.Inventory inventory = new UI.Components.Inventory();
      Hotbar overlayHotbar = new Hotbar { isMain = false };

      overlay.rootVisualElement.Q("inventory").Add(inventory);
      overlay.rootVisualElement.Q("holder").Add(overlayHotbar);

      Hotbar hudHotbar = new Hotbar { isMain = true };
      hud.rootVisualElement.Q("hotbar-holder").Add(hudHotbar);
      #endregion
    }

    private void CheckOverlay()
    {
      #region CheckOverlay
      if (playerInput.UI.OpenInventory.WasPressedThisFrame())
      {
        isOverlayOpen = !isOverlayOpen;

        if (isOverlayOpen) OpenOverlay(-1, OverlayType.Inventory);
        else CloseOverlay();
        return;
      }

      if (playerInput.UI.CloseOverlay.WasPressedThisFrame() && isOverlayOpen) CloseOverlay();
      #endregion
    }

    public void OpenOverlay(int blockId, OverlayType overlayType, object data = null)
    {
      #region OpenOverlay
      hud.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.None;
      overlay.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.Flex;
      isOverlayOpen = true;

      var (overlayData, overlayElement) = GetOverlay(overlayType);
      VisualElement left = overlay.rootVisualElement.Q("left");
      if (left.childCount != 0) left.RemoveAt(0);
      left.Add(overlayElement);

      OnOverlayOpen?.Invoke(blockId, overlayType, data);
      overlayData.RefreshUI();
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

    /// <summary>Method</summary>
    private (Overlay, VisualElement) GetOverlay(OverlayType overlayType)
    {
      #region GetSourceTree
      switch (overlayType)
      {
        case OverlayType.Inventory:
          {
            Items.Overlays.Equipment data;
            if (equipmentOverlay != null) data = equipmentOverlay;
            else data = new Items.Overlays.Equipment();

            VisualElement element = new Equipment(data);
            equipmentOverlay = data;
            return (data, element);
          }
        default:
          return (null, null);
      }
      #endregion
    }
    #endregion
  }
}
