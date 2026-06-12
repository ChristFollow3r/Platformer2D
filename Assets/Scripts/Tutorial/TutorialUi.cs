using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class TutorialUI : MonoBehaviour
{
    private UIDocument document;
    private VisualElement popupContainer;
    private Label descriptionLabel;
    private Button closeButton;
    private VisualElement goalTrackerContainer;

    [Header("Styling")]
    [SerializeField] private Font customFont;
    [SerializeField] private AudioClip goalCompleteSound;

    [Header("Toggle Sprites")]
    [SerializeField] private Sprite uncheckedSprite;
    [SerializeField] private Sprite checkedSprite;

    public static bool IsInputBlocked { get; private set; }
    private float unblockTimer = 0f;
    private bool isPopupVisible = false;

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        popupContainer = document.rootVisualElement.Q<VisualElement>("VideoPopup");
        descriptionLabel = document.rootVisualElement.Q<Label>("Description");
        closeButton = document.rootVisualElement.Q<Button>("CloseButton");
        goalTrackerContainer = document.rootVisualElement.Q<VisualElement>("GoalTracker");

        if (closeButton != null)
        {
            closeButton.clicked += HidePopup;
        }

        HidePopup();
    }

    private void Update()
    {
        if (unblockTimer > 0f)
        {
            unblockTimer -= Time.deltaTime;
            if (unblockTimer <= 0f)
            {
                IsInputBlocked = false;
                isPopupVisible = false;
            }
        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame && isPopupVisible) HidePopup();
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.clicked -= HidePopup;
        }
        IsInputBlocked = false;
    }

    public void ShowPopup(string textDescription)
    {
        Time.timeScale = 0f;
        IsInputBlocked = true;
        isPopupVisible = true;
        Player.UIController.Singleton.preventMenu = true;
        unblockTimer = 0f;

        if (descriptionLabel != null)
        {
            descriptionLabel.text = textDescription;
        }
        popupContainer.style.display = DisplayStyle.Flex;
    }

    public void HidePopup()
    {
        Time.timeScale = 1f;
        Player.UIController.Singleton.preventMenu = false;
        popupContainer.style.display = DisplayStyle.None;
        unblockTimer = 0.2f;
    }

    public void ClearGoals()
    {
        if (goalTrackerContainer != null)
        {
            goalTrackerContainer.Clear();
        }
    }

    public void AddGoal(string goalId, string labelText)
    {
        if (goalTrackerContainer == null) return;

        Toggle existingToggle = goalTrackerContainer.Q<Toggle>(goalId);
        if (existingToggle != null) return;

        VisualElement goalRow = new VisualElement();
        goalRow.style.flexDirection = FlexDirection.Row;
        goalRow.style.justifyContent = Justify.SpaceBetween;
        goalRow.style.alignItems = Align.Center;
        goalRow.style.width = Length.Percent(100);

        Label goalLabel = new Label(labelText);
        goalLabel.style.color = Color.white;

        if (customFont != null)
        {
            goalLabel.style.unityFont = customFont;
            goalLabel.style.unityFontDefinition = StyleKeyword.None;
        }

        Toggle newGoal = new Toggle();
        newGoal.name = goalId;
        newGoal.focusable = false;

        newGoal.RegisterCallback<GeometryChangedEvent>(evt =>
        {
            VisualElement checkmark = newGoal.Q(className: "unity-toggle__checkmark");
            if (checkmark != null)
            {
                checkmark.style.backgroundColor = Color.clear;
                checkmark.style.borderTopWidth = 0;
                checkmark.style.borderBottomWidth = 0;
                checkmark.style.borderLeftWidth = 0;
                checkmark.style.borderRightWidth = 0;

                checkmark.style.unityBackgroundImageTintColor = Color.white;

                if (uncheckedSprite != null)
                    checkmark.style.backgroundImage = new StyleBackground(uncheckedSprite);
            }
        });

        newGoal.RegisterValueChangedCallback(evt =>
        {
            VisualElement checkmark = newGoal.Q(className: "unity-toggle__checkmark");
            if (checkmark != null)
            {
                Sprite spriteToUse = evt.newValue ? checkedSprite : uncheckedSprite;
                if (spriteToUse != null)
                {
                    checkmark.style.backgroundImage = new StyleBackground(spriteToUse);
                }
            }
        });

        goalRow.Add(goalLabel);
        goalRow.Add(newGoal);

        goalTrackerContainer.Add(goalRow);
    }

    public void CompleteGoal(string toggleName, bool quiet = false)
    {
        Toggle toggleElement = document.rootVisualElement.Q<Toggle>(toggleName);
        if (toggleElement == null) return;

        if (!toggleElement.value)
        {
            toggleElement.value = true;
            if (quiet) return;
            if (goalCompleteSound != null && Camera.main != null)
            {
                AudioSource.PlayClipAtPoint(goalCompleteSound, Camera.main.transform.position);
            }
        }
    }
}
