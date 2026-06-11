using Player;
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
        UIController.Singleton.preventMenu = true;
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
        UIController.Singleton.preventMenu = false;
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

        goalRow.Add(goalLabel);
        goalRow.Add(newGoal);

        goalTrackerContainer.Add(goalRow);
    }

    public void CompleteGoal(string toggleName)
    {
        Toggle toggleElement = document.rootVisualElement.Q<Toggle>(toggleName);
        if (!toggleElement.value)
        {
            toggleElement.value = true;
            if (goalCompleteSound != null && Camera.main != null)
            {
                AudioSource.PlayClipAtPoint(goalCompleteSound, Camera.main.transform.position);
            }
        }
    }
}
