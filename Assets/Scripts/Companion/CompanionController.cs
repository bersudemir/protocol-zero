using System.Collections;
using UnityEngine;
using UnityEngine.XR;
[RequireComponent(typeof(CompanionNavigator))]
[RequireComponent(typeof(CompanionStateMachine))]
public class CompanionController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float resumeFollowDistance = 2.5f;
    [SerializeField] private float targetArrivalDistance = 0.7f;
    [SerializeField] private float interactionDuration = 1.5f;
    private CompanionNavigator navigator;
    private CompanionStateMachine stateMachine;
    private Transform currentTarget;

    private System.Action onInteractionCompleted;
    private void Awake()
    {
        navigator = GetComponent<CompanionNavigator>();
        stateMachine = GetComponent<CompanionStateMachine>();
    }
    private void Update()
    {
        if (player == null)
            return;
        switch (stateMachine.CurrentState)
        {
            case CompanionState.Following:
                HandleFollowing();
                break;

            case CompanionState.Waiting:
                HandleWaiting();
                break;

            case CompanionState.GoingToTarget:
                HandleGoingToTarget();
                break;

            case CompanionState.Returning:
                HandleReturning();
                break;

        }
    }

    private void HandleFollowing()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > followDistance)
        {
            navigator.MoveTo(player);
        }
        else
        {
            navigator.Stop();
            stateMachine.ChangeState(CompanionState.Waiting);
        }
    }

    private void HandleWaiting()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > resumeFollowDistance)
        {
            stateMachine.ChangeState(CompanionState.Following);
            navigator.MoveTo(player);
        }
    }

    private void HandleGoingToTarget()
    {
        if (currentTarget == null)
        {
            navigator.Stop();
            stateMachine.ChangeState(CompanionState.Failed);
            return;
        }
        navigator.MoveTo(currentTarget);
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (distanceToTarget <= targetArrivalDistance)
        {
            navigator.Stop();
            stateMachine.ChangeState(CompanionState.Interacting);
            StartCoroutine(CompleteInteraction());
        }
    }
    private IEnumerator CompleteInteraction()
    {
        Debug.Log("Companion ile panel etkileşime başladı.");
        yield return new WaitForSeconds(interactionDuration);
        if (stateMachine.CurrentState != CompanionState.Interacting)
            yield break;

        Debug.Log("Companion paneldeki etkileşim süresini tamamladı.");
        System.Action completedAction = onInteractionCompleted;
        onInteractionCompleted = null;
        currentTarget = null;
        stateMachine.ChangeState(CompanionState.Returning);
        completedAction?.Invoke();
    }
    private void HandleReturning()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= followDistance)
        {
            navigator.Stop();
            Debug.Log("Companion oyuncuya geri döndü");
            stateMachine.ChangeState(CompanionState.Waiting);
            return;
        }
        navigator.MoveTo(player);
    }
    public void GoToTarget(Transform target, System.Action onCompleted = null)
    {
        if (target == null)
        {
            navigator.Stop();
            stateMachine.ChangeState(CompanionState.Failed);
            return;
        }

        if(stateMachine.CurrentState == CompanionState.GoingToTarget ||
           stateMachine.CurrentState == CompanionState.Interacting ||
           stateMachine.CurrentState == CompanionState.Returning )
        {
            Debug.LogWarning("Companion zaten bir görev gerçekleştiriyor.");
            return;
        }
        onInteractionCompleted = onCompleted;
        currentTarget = target;
        stateMachine.ChangeState(CompanionState.GoingToTarget);
        navigator.MoveTo(currentTarget);
    }
}
