using System;
using UnityEngine;
using UnityEngine.UIElements;


namespace Player
{
  public enum OverlayType
  {
    Inventory
  };


  public class UIController : MonoBehaviour
  {
    #region Data
    public InputSystem_Actions playerInput;
    public UIDocument overlay;
    public UIDocument hud;

    [SerializeField] private bool isOverlayOpen;
    #endregion

    #region Events
    public static event Action<OverlayType> OnOverlayOpen;
    public static event Action OnOverlayClose;
    #endregion

    #region Unity
    /// <summary>Ran by unity on load</summary>
    private void Awake()
    {
      #region Awake
      playerInput = new InputSystem_Actions();
      playerInput.Enable();
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
    #endregion


    private void OpenOverlay(OverlayType overlayType)
    {
      #region OpenOverlay
      OnOverlayOpen?.Invoke(overlayType);
      // TODO: set type of overlay for ui
      hud.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.None;
      overlay.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.Flex;
      isOverlayOpen = true;
      #endregion
    }


    private void CloseOverlay()
    {
      #region CloseOverlay
      OnOverlayClose?.Invoke();
      hud.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.Flex;
      overlay.rootVisualElement.Q<VisualElement>("root").style.display = DisplayStyle.None;
      isOverlayOpen = false;
      #endregion
    }
  }
}
