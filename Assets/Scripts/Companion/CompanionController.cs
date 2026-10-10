using UnityEngine;
[RequireComponent(typeof(CompanionNavigator))]
[RequireComponent(typeof(CompanionStateMachine))]
public class CompanionController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float followDistance = 2f;
    [SerializeField] private float resumeFollowDistance = 2.5f;
    private CompanionNavigator navigator;
    private CompanionStateMachine stateMachine;
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
}
