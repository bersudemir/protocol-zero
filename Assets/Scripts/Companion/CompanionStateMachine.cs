using UnityEngine;
public enum CompanionState
{
    Following,
    Waiting,
    GoingToTarget,
    Interacting,
    CarryingItem,
    Returning,
    Failed
}

public class CompanionStateMachine : MonoBehaviour
{
    [SerializeField] private CompanionState currentState = CompanionState.Following;
    public CompanionState CurrentState => currentState;
    public void ChangeState(CompanionState newState)
    {
        if (currentState == newState)
            return;
        currentState = newState;
        Debug.Log("Companion durumu: " + currentState);
    }
}
