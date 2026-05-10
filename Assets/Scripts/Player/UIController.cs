using System;
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
    #region Data
    [Header("Elements")]
    [SerializeField] private GameObject uiHolder;
    [SerializeField] private UIDocument overlay;
    [SerializeField] private UIDocument hud;
    private VisualElement overlayRoot;
    private VisualElement hudRoot;

    [Header("Controls")]
    [SerializeField] private bool isOverlayOpen;
    private InputSystem_Actions playerInput;
    #endregion

    #region Events
    public static event Action<OverlayType, object> OnOverlayOpen;
    public static event Action OnOverlayClose;
    #endregion

    #region Unity
    /// <summary>Ran by unity on load</summary>
    private void Awake()
    {
      #region Awake
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
      Inventory inventory = new Inventory();
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

        if (isOverlayOpen) OpenOverlay(OverlayType.Inventory);
        else CloseOverlay();
        return;
      }

      if (playerInput.UI.CloseOverlay.WasPressedThisFrame() && isOverlayOpen) CloseOverlay();
      #endregion
    }

    public void OpenOverlay(OverlayType overlayType, object data = null)
    {
      #region OpenOverlay
      hud.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.None;
      overlay.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.Flex;
      isOverlayOpen = true;



      OnOverlayOpen?.Invoke(overlayType, data);
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
    #endregion
  }
}
