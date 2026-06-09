using UnityEngine;
using UnityEngine.UIElements;

public class TutorialUI : MonoBehaviour
{
    private UIDocument document;
    private VisualElement popupContainer;

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        popupContainer = document.rootVisualElement.Q<VisualElement>("VideoPopup");
        HidePopup();
    }

    public void ShowPopup()
    {
        popupContainer.style.display = DisplayStyle.Flex;
    }

    public void HidePopup()
    {
        popupContainer.style.display = DisplayStyle.None;
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
