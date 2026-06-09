using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using Player;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialState
    {
        Movement,
        Attacking,
        Mining,
        Placing,
        Completed
    }

    private TutorialState currentState = TutorialState.Movement;

    public TutorialUI tutorialUI;
    public VideoPlayer videoPlayer;
    public VideoClip movementClip;
    public VideoClip attackingClip;
    public VideoClip miningClip;
    public VideoClip placingClip;

    private PlayerMovement playerMovement;
    private BreakAndPlace breakAndPlace;

    private bool isWaitingToAdvance = false;

    private void Start()
    {
        StartCoroutine(WaitForPlayer());
    }

    private void OnDestroy()
    {
        if (playerMovement != null)
        {
            playerMovement.OnMovePerformed -= HandleMove;
            playerMovement.OnAttackPerformed -= HandleAttack;
        }

        if (breakAndPlace != null)
        {
            breakAndPlace.OnBlockBroken -= HandleMine;
            breakAndPlace.OnPlacePerformed -= HandlePlace;
        }
    }

    private IEnumerator WaitForPlayer()
    {
        while (PlayerMovement.Singleton == null)
        {
            yield return null;
        }

        playerMovement = PlayerMovement.Singleton;
        breakAndPlace = playerMovement.GetComponent<BreakAndPlace>();

        if (playerMovement != null)
        {
            playerMovement.OnMovePerformed += HandleMove;
            playerMovement.OnAttackPerformed += HandleAttack;
        }

        if (breakAndPlace != null)
        {
            breakAndPlace.OnBlockBroken += HandleMine;
            breakAndPlace.OnPlacePerformed += HandlePlace;
        }

        ProcessCurrentState();
    }

    private void HandleMove()
    {
        if (currentState == TutorialState.Movement && !isWaitingToAdvance)
        {
            StartCoroutine(AdvanceTutorialWithDelay(1f));
        }
    }

    private void HandleAttack(Vector2 pos)
    {
        if (currentState == TutorialState.Attacking && !isWaitingToAdvance)
        {
            StartCoroutine(AdvanceTutorialWithDelay(1f));
        }
    }

    private void HandleMine()
    {
        if (currentState == TutorialState.Mining && !isWaitingToAdvance)
        {
            StartCoroutine(AdvanceTutorialWithDelay(1f));
        }
    }

    private void HandlePlace()
    {
        if (currentState == TutorialState.Placing && !isWaitingToAdvance)
        {
            StartCoroutine(AdvanceTutorialWithDelay(1f));
        }
    }

    private IEnumerator AdvanceTutorialWithDelay(float delayTime)
    {
        isWaitingToAdvance = true;
        yield return new WaitForSeconds(delayTime);
        AdvanceTutorial();
        isWaitingToAdvance = false;
    }

    public void AdvanceTutorial()
    {
        if (currentState != TutorialState.Completed)
        {
            currentState++;
            ProcessCurrentState();
        }
    }

    private void ProcessCurrentState()
    {
        switch (currentState)
        {
            case TutorialState.Movement:
                videoPlayer.clip = movementClip;
                videoPlayer.Play();
                tutorialUI.ShowPopup("Use A and D to move. SPACE to jump and double jump.");
                tutorialUI.ShowGoal("MovementToggle");
                break;
            case TutorialState.Attacking:
                tutorialUI.CompleteGoal("MovementToggle");
                videoPlayer.clip = attackingClip;
                videoPlayer.Play();
                tutorialUI.ShowPopup("Left Click to attack enemies or hit props.");
                tutorialUI.ShowGoal("AttackingToggle");
                break;
            case TutorialState.Mining:
                tutorialUI.CompleteGoal("AttackingToggle");
                videoPlayer.clip = miningClip;
                videoPlayer.Play();
                tutorialUI.ShowPopup("Hold Shift + Left Click to mine blocks.");
                tutorialUI.ShowGoal("MiningToggle");
                break;
            case TutorialState.Placing:
                tutorialUI.CompleteGoal("MiningToggle");
                videoPlayer.clip = placingClip;
                videoPlayer.Play();
                tutorialUI.ShowPopup("Right Click to place a block.");
                tutorialUI.ShowGoal("PlacingBlock");
                break;
            case TutorialState.Completed:
                tutorialUI.HidePopup();
                tutorialUI.CompleteGoal("PlacingBlock");
                break;
        }
    }
}
