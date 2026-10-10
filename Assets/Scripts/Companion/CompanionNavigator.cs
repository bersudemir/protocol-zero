using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]

public class CompanionNavigator : MonoBehaviour
{
    private NavMeshAgent agent;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    public void MoveTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("Companion hedefi atanmadı.");
            return;
        }
        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("Companion NavMesh üzerinde değil");
            return;
        }
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }
    public void Stop()
    {
        if (!agent.isOnNavMesh)
            return;
        agent.isStopped = true;
    }

}
