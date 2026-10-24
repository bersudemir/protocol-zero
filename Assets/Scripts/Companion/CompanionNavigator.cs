using UnityEngine;
using UnityEngine.AI;
[RequireComponent(typeof(NavMeshAgent))]

public class CompanionNavigator : MonoBehaviour
{
    [Header("Rota Güncelleme")]
    [SerializeField, Min(0.05f)]
    private float repathInterval = 0.25f;
    [SerializeField, Min(0.01f)]
    private float destinationChangeThreshold = 0.2f;
    [SerializeField]
    private bool drawPathGizmos = true;
    private NavMeshAgent agent;
    private bool hasLastDestination;
    private Vector3 lastDestination;
    private float lastRepathTime = -100f;
    public bool IsOnNavMesh =>
        agent != null && agent.isOnNavMesh;
    public bool IsPathPending =>
       IsOnNavMesh&&agent.pathPending;
    public bool HasPath =>
        IsOnNavMesh && agent.hasPath;
    public NavMeshPathStatus PathStatus =>
       IsOnNavMesh
        ? agent.pathStatus
        : NavMeshPathStatus.PathInvalid;
    public float RemainingDistance =>
        IsOnNavMesh
        ? agent.remainingDistance
        : float.PositiveInfinity;
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    public void MoveTo(Transform target)
    {
        string error;
        if(!TryMoveTo(target,out error))
        {
            Debug.LogWarning(
                "Companion Navigasyon: " + error);
        }
    }
    public bool TryMoveTo(
        Transform target,
        out string error)
    {
        error = null;
        if (target == null)
        {
            error = "Hedef atanmadı.";
            return false;
        }
        if (!IsOnNavMesh)
        {
            error = "Companion NavMesh üzerinde değil.";
            return false;
        }
        if (agent.isStopped)
        {
            agent.isStopped = false;
        }
        Vector3 desired = target.position;
        bool moved =
            !hasLastDestination ||
            Vector3.Distance(
                desired,
                lastDestination
                ) >= destinationChangeThreshold;
        bool lostPath =
            !agent.hasPath &&
            !agent.pathPending &&
            Time.time - lastRepathTime >= 1f;
        bool cooldownOver = Time.time - lastRepathTime >= repathInterval;
        if ((moved || lostPath) && cooldownOver)
        {
            if (!agent.SetDestination(desired))
            {
                error = "NavMeshAgent.SetDestination hedefi kabul etmedi.";
                return false;
            }
            lastDestination = desired;
            hasLastDestination = true;
            lastRepathTime = Time.time;
        }
        return true;
    }
    public void Stop()
    {
        if (!IsOnNavMesh)
        {
            return;
        }
        agent.isStopped = true;
        agent.ResetPath();
        hasLastDestination = false;
    }
    private void OnDrawGizmosSelected()
    {
        if (!drawPathGizmos)
        {
            return;
        }

        NavMeshAgent current =
            agent != null
                ? agent
                : GetComponent<NavMeshAgent>();
        if(current==null ||
            !current.isOnNavMesh||
            !current.hasPath)
        {
            return;
        }
        Gizmos.color=current.pathStatus==NavMeshPathStatus.PathComplete
            ?Color.cyan
            :Color.red;
        Vector3[] corners = current.path.corners;
        for(int i=1; i<corners.Length; i++)
        {
            Gizmos.DrawLine(
                corners[i - 1],
                corners[i]);
            Gizmos.DrawSphere(
                corners[i],
                0.08f);
        }
        
    }

}
