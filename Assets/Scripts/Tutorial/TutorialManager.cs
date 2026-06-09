using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Video;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialState
    {
        Movement,
        Mining,
        Furnace,
        Completed
    }

    private TutorialState currentState = TutorialState.Movement;
    public TutorialUI tutorialUI;
    public VideoPlayer videoPlayer;
    public VideoClip movementClip;
    public VideoClip miningClip;

    private void Start()
    {
        ProcessCurrentState();
    }

    public void AdvanceTutorial()
    {
        currentState++;
        ProcessCurrentState();
    }

    private void ProcessCurrentState()
    {
        switch (currentState)
        {
            case TutorialState.Movement:
                videoPlayer.clip = movementClip;
                videoPlayer.Play();
                tutorialUI.ShowPopup();
                tutorialUI.ShowGoal("MovementToggle");
                break;
            case TutorialState.Mining:
                tutorialUI.CompleteGoal("MovementToggle");
                videoPlayer.clip = miningClip;
                videoPlayer.Play();
                tutorialUI.ShowPopup();
                tutorialUI.ShowGoal("MiningToggle");
                break;
            case TutorialState.Furnace:
                tutorialUI.HidePopup();
                tutorialUI.CompleteGoal("MiningToggle");
                tutorialUI.ShowGoal("FurnaceToggle");
                break;
            case TutorialState.Completed:
                tutorialUI.CompleteGoal("FurnaceToggle");
                break;
        }
    }
}
