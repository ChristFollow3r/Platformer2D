using System;
using System.Collections.Generic;
using Items;
using UI.Components;
using UnityEngine;
using UnityEngine.UIElements;
using Data;
using Scriptable_Objects_Scripts;

namespace Player
{
    public enum OverlayType
    {
        Inventory,
        Furnace,
        Chest
    };

    [DefaultExecutionOrder(-50)]
    public class UIController : MonoBehaviour
    {
        #region Singleton setup

        public static UIController Singleton;

        private void SetupSingleton()
        {
            #region SetupSingleton

            if (Singleton != null && Singleton != this)
            {
                Destroy(gameObject);
                return;
            }

            Singleton = this;

            #endregion
        }

        #endregion

        #region Data

        [Header("Menu Elements")]
        [SerializeField] private UIDocument pauseMenu;

        public bool isMenuOpen = false;

        [Header("Elements")]
        [SerializeField] private UIDocument overlay;
        [SerializeField] private UIDocument hud;

        private VisualElement overlayRoot;
        private VisualElement hudRoot;

        private Dictionary<ulong, Overlay> overlaysByBlockId = new();

        [Header("Controls")]
        public bool isOverlayOpen;
        private InputSystem_Actions playerInput;

        #endregion

        #region Events

        public event Action<ulong, OverlayType> OnOverlayOpen;
        public event Action OnOverlayClose;

        #endregion

        #region Unity

        private void Awake()
        {
            #region Awake

            SetupSingleton();
            playerInput = new InputSystem_Actions();
            playerInput.Enable();
            CreateUI();

            #endregion
        }

        private void Start()
        {
            #region Start

            CloseOverlay();

            #endregion
        }

        private void Update()
        {
            #region Update

            CheckOverlay();
            MoveHand();
            foreach (Overlay overlay in overlaysByBlockId.Values)
            {
                overlay.Tick();
            }

            #endregion
        }

        #endregion

        #region Recipe Book Data

        [Header("Recipe Book")]
        [SerializeField] private RecipeDatabase craftingDatabase;
        [SerializeField] private CookingRecipeDatabase cookingDatabase;

        private int currentRecipeIndex = 0;

        private VisualElement recipeBookContainer;

        // Left Page UI
        private Label leftTitleText;
        private VisualElement leftGridContainer;
        private Button prevPageBtn;

        // Right Page UI
        private Label rightTitleText;
        private VisualElement rightGridContainer;
        private Button nextPageBtn;

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

            // MENU
            pauseMenu.rootVisualElement.style.display = DisplayStyle.None;

            Button openMenuBtn = hud.rootVisualElement.Q<Button>("OpenMenu");
            if (openMenuBtn != null) openMenuBtn.clicked += ToggleMenu;

            var menuRoot = pauseMenu.rootVisualElement;

            InitializeRecipeBook(menuRoot);

            menuRoot.Q<Button>("Settings").clicked += () => Debug.Log("Settings");
            menuRoot.Q<Button>("Recipes").clicked += OpenRecipeBook;
            menuRoot.Q<Button>("Save").clicked += () => Debug.Log("Save");
            menuRoot.Q<Button>("Exit").clicked += () => Application.Quit();

            #endregion
        }

        public void ToggleMenu()
        {
            isMenuOpen = !isMenuOpen;

            if (isMenuOpen)
            {
                pauseMenu.rootVisualElement.style.display = DisplayStyle.Flex;
                hud.rootVisualElement.style.display = DisplayStyle.None;
            }
            else
            {
                pauseMenu.rootVisualElement.style.display = DisplayStyle.None;
                hud.rootVisualElement.style.display = DisplayStyle.Flex;
            }
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

        #region Recipe Book Logic

        private void InitializeRecipeBook(VisualElement menuRoot)
        {
            recipeBookContainer = menuRoot.Q<VisualElement>("RecipeBookContainer");
            if (recipeBookContainer == null) return;

            leftTitleText = recipeBookContainer.Q<Label>("LeftPageTitle");
            leftGridContainer = recipeBookContainer.Q<VisualElement>("LeftPageGrid");
            prevPageBtn = recipeBookContainer.Q<Button>("PrevPage");

            rightTitleText = recipeBookContainer.Q<Label>("RightPageTitle");
            rightGridContainer = recipeBookContainer.Q<VisualElement>("RightPageGrid");
            nextPageBtn = recipeBookContainer.Q<Button>("NextPage");

            if (nextPageBtn != null) nextPageBtn.clicked += () => TurnPage(1);
            if (prevPageBtn != null) prevPageBtn.clicked += () => TurnPage(-1);

            recipeBookContainer.style.display = DisplayStyle.None;
        }

        private int GetTotalRecipes()
        {
            int craftingCount = craftingDatabase != null && craftingDatabase.recipes != null ? craftingDatabase.recipes.Count : 0;
            int cookingCount = cookingDatabase != null && cookingDatabase.recipes != null ? cookingDatabase.recipes.Count : 0;
            return craftingCount + cookingCount;
        }

        private void OpenRecipeBook()
        {
            if (GetTotalRecipes() == 0) return;

            recipeBookContainer.style.display = DisplayStyle.Flex;
            currentRecipeIndex = 0;
            DisplayPages();
        }

        private void TurnPage(int direction)
        {
            int totalRecipes = GetTotalRecipes();
            int newIndex = currentRecipeIndex + (direction * 2);

            if (newIndex >= 0 && newIndex < totalRecipes)
            {
                currentRecipeIndex = newIndex;
                DisplayPages();
            }
        }

        private void DisplayPages()
        {
            int totalRecipes = GetTotalRecipes();

            PopulatePageData(currentRecipeIndex, leftTitleText, leftGridContainer);

            if (currentRecipeIndex + 1 < totalRecipes)
            {
                rightTitleText.style.display = DisplayStyle.Flex;
                rightGridContainer.style.display = DisplayStyle.Flex;
                PopulatePageData(currentRecipeIndex + 1, rightTitleText, rightGridContainer);
            }
            else
            {
                if (rightTitleText != null) rightTitleText.text = "";
                if (rightGridContainer != null) rightGridContainer.Clear();
            }

            if (prevPageBtn != null)
                prevPageBtn.style.display = (currentRecipeIndex == 0) ? DisplayStyle.None : DisplayStyle.Flex;

            if (nextPageBtn != null)
                nextPageBtn.style.display = (currentRecipeIndex + 2 >= totalRecipes) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void PopulatePageData(int index, Label titleLabel, VisualElement gridContainer)
        {
            if (titleLabel == null || gridContainer == null) return;

            gridContainer.Clear();

            // Force the container to stack everything vertically and center it
            gridContainer.style.flexDirection = FlexDirection.Column;
            gridContainer.style.alignItems = Align.Center;

            int craftingCount = craftingDatabase != null && craftingDatabase.recipes != null ? craftingDatabase.recipes.Count : 0;

            ItemData resultItem = null;
            ItemData[] ingredientsArray = null;
            int displayColumns = 4;

            if (index < craftingCount)
            {
                var recipe = craftingDatabase.recipes[index];
                resultItem = recipe.result;
                ingredientsArray = recipe.ingredients;
                displayColumns = 4;
                titleLabel.text = "Crafting Table";
            }
            else
            {
                int cookingIndex = index - craftingCount;
                var recipe = cookingDatabase.recipes[cookingIndex];
                resultItem = recipe.result;
                ingredientsArray = recipe.ingredients;
                displayColumns = recipe.gridSize;
                titleLabel.text = "Furnace";
            }

            // --- 1. DRAW INGREDIENT GRID FIRST ---
            VisualElement gridWrapper = new VisualElement();
            float slotSize = 40f;

            gridWrapper.style.width = (displayColumns * slotSize) + 4;
            gridWrapper.style.flexDirection = FlexDirection.Row;
            gridWrapper.style.flexWrap = Wrap.Wrap;
            gridWrapper.style.justifyContent = Justify.Center;
            gridWrapper.style.marginBottom = 20; // Adds a nice gap between the grid and the result

            for (int i = 0; i < ingredientsArray.Length; i++)
            {
                VisualElement slot = new VisualElement();
                slot.style.width = slotSize;
                slot.style.height = slotSize;

                slot.style.borderTopWidth = 1;
                slot.style.borderBottomWidth = 1;
                slot.style.borderLeftWidth = 1;
                slot.style.borderRightWidth = 1;
                slot.style.borderTopColor = new StyleColor(Color.black);
                slot.style.borderBottomColor = new StyleColor(Color.black);
                slot.style.borderLeftColor = new StyleColor(Color.black);
                slot.style.borderRightColor = new StyleColor(Color.black);
                slot.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.2f));

                if (ingredientsArray[i] != null && ingredientsArray[i].sprite != null)
                {
                    Image icon = new Image();
                    icon.sprite = ingredientsArray[i].sprite;
                    icon.style.width = Length.Percent(100);
                    icon.style.height = Length.Percent(100);
                    slot.Add(icon);
                }

                gridWrapper.Add(slot);
            }

            gridContainer.Add(gridWrapper);

            // --- 2. DRAW RESULT ICON AND LABEL BELOW ---
            VisualElement resultWrapper = new VisualElement();
            resultWrapper.style.alignItems = Align.Center; // Centers the box and label
            resultWrapper.style.flexDirection = FlexDirection.Column; // Stacks box and label vertically

            VisualElement resultBox = new VisualElement();
            resultBox.style.width = 60;
            resultBox.style.height = 60;
            resultBox.style.borderTopWidth = 2;
            resultBox.style.borderBottomWidth = 2;
            resultBox.style.borderLeftWidth = 2;
            resultBox.style.borderRightWidth = 2;
            resultBox.style.borderTopColor = new StyleColor(Color.black);
            resultBox.style.borderBottomColor = new StyleColor(Color.black);
            resultBox.style.borderLeftColor = new StyleColor(Color.black);
            resultBox.style.borderRightColor = new StyleColor(Color.black);
            resultBox.style.backgroundColor = new StyleColor(new Color(0, 0, 0, 0.4f));

            if (resultItem != null && resultItem.sprite != null)
            {
                Image resIcon = new Image();
                resIcon.sprite = resultItem.sprite;
                resIcon.style.width = Length.Percent(100);
                resIcon.style.height = Length.Percent(100);
                resultBox.Add(resIcon);
            }
            resultWrapper.Add(resultBox);

            // Add the text label below the box
            if (resultItem != null)
            {
                Label resultLabel = new Label();
                resultLabel.text = resultItem.name;
                resultLabel.style.marginTop = 5;
                resultLabel.style.color = new StyleColor(Color.black);
                resultLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

                resultWrapper.Add(resultLabel);
            }

            gridContainer.Add(resultWrapper);
        }

        #endregion
    }
}
