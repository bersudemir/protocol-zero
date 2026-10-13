using UnityEngine;
[RequireComponent(typeof(CompanionNavigator))]
[RequireComponent(typeof(CompanionStateMachine))]
public class CompanionController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float resumeFollowDistance = 2.5f;
    [SerializeField] private float targetArrivalDistance = 0.7f;
    private CompanionNavigator navigator;
    private CompanionStateMachine stateMachine;
    private Transform currentTarget;
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
            stateMachine.ChangeState(CompanionState.Failed);
            navigator.Stop();
            return;
        }
        navigator.MoveTo(currentTarget);
        float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

        if (distanceToTarget <= targetArrivalDistance)
        {
            navigator.Stop();
            stateMachine.ChangeState(CompanionState.Interacting);
        }
    }

    public void GoToTarget(Transform target)
    {
        if (target == null)
        {
            stateMachine.ChangeState(CompanionState.Failed);
            return;
        }

        currentTarget = target;
        stateMachine.ChangeState(CompanionState.GoingToTarget);
        navigator.MoveTo(currentTarget);
    }
}
