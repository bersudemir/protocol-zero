using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]

public class CompanionNavigator : MonoBehaviour
{
    [SerializeField] private Transform target;
    private NavMeshAgent agent;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    private void Start()
    {
        MoveToTarget();
    }
    public void MoveToTarget()
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
        agent.SetDestination(target.position);
    }
}
