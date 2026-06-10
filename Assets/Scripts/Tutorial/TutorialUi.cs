using UnityEngine;
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
            }
        }
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
        IsInputBlocked = true;
        unblockTimer = 0f;

        if (descriptionLabel != null)
        {
            descriptionLabel.text = textDescription;
        }
        popupContainer.style.display = DisplayStyle.Flex;
    }

    public void HidePopup()
    {
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

        Toggle newGoal = new Toggle();
        newGoal.name = goalId;
        newGoal.label = labelText;
        newGoal.focusable = false;

        newGoal.style.color = Color.white;

        if (customFont != null)
        {
            newGoal.style.unityFont = customFont;
            // This prevents Unity's default TMP asset from overriding your custom TTF/OTF font
            newGoal.style.unityFontDefinition = StyleKeyword.None;
        }

        goalTrackerContainer.Add(newGoal);
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
