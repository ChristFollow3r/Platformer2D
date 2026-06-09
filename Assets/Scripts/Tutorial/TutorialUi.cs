using UnityEngine;
using UnityEngine.UIElements;

public class TutorialUI : MonoBehaviour
{
    private UIDocument document;
    private VisualElement popupContainer;
    private Label descriptionLabel;
    private Button closeButton;

    public static bool IsInputBlocked { get; private set; }
    private float unblockTimer = 0f;

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        popupContainer = document.rootVisualElement.Q<VisualElement>("VideoPopup");
        descriptionLabel = document.rootVisualElement.Q<Label>("Description");
        closeButton = document.rootVisualElement.Q<Button>("CloseButton");

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

    public void ShowGoal(string toggleName)
    {
        Toggle toggleElement = document.rootVisualElement.Q<Toggle>(toggleName);
        if (toggleElement != null)
        {
            toggleElement.style.display = DisplayStyle.Flex;
        }
    }

    public void CompleteGoal(string toggleName)
    {
        Toggle toggleElement = document.rootVisualElement.Q<Toggle>(toggleName);
        if (toggleElement != null)
        {
            toggleElement.value = true;
        }
    }
}
