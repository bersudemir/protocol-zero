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
    [SerializeField] private float targetTimeoutSeconds = 30f;
    private float targetStartTime;
    private CompanionNavigator navigator;
    private CompanionStateMachine stateMachine;
    private Transform currentTarget;
    private float noPathSince = -1f;
    private float lastProgressDistance = float.PositiveInfinity;
    private float lastProgressTime = 0f;
    private float failedSince = -1f;

    private System.Action onInteractionCompleted;
    private System.Action onReturnedToPlayer;
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

            case CompanionState.Failed:
                HandleFailed();
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
        if(Time.time - targetStartTime >= targetTimeoutSeconds)
        {
            Debug.LogWarning("Companion: Hedefe ulaşma süresi aşıldı.");
            navigator.Stop();
            currentTarget = null;
            onInteractionCompleted = null;
            stateMachine.ChangeState(CompanionState.Failed);
            return;
        }
        if(!navigator.TryMoveTo(currentTarget,out string navigationError))
        {
            Debug.LogWarning(
                "Companion navigasyon hatası: " + navigationError);
            navigator.Stop();
            currentTarget = null;
            onInteractionCompleted = null;
            stateMachine.ChangeState(CompanionState.Failed);
            return;
        }

        if(!navigator.IsPathPending&&
            navigator.HasPath&&
            navigator.PathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning("Companion geçersiz veya eksik rota: " + navigator.PathStatus);
            navigator.Stop();
            currentTarget = null;
            onInteractionCompleted = null;
            stateMachine.ChangeState(CompanionState.Failed);
            return;
        }

        if (!navigator.IsPathPending && !navigator.HasPath)
        {
            if (noPathSince < 0f)
            {
                noPathSince = Time.time;
            }
            else if (Time.time - noPathSince >= 2f)
            {
                Debug.LogWarning("Companion: Hedefe giden NavMesh yolu bulunamadı.");
                navigator.Stop();
                currentTarget = null;
                onInteractionCompleted = null;
                noPathSince = -1f;
                stateMachine.ChangeState(CompanionState.Failed);
                return;
            }
        }
        else
        {
            noPathSince = -1f;
        }

        if(!navigator.IsPathPending&&
            navigator.HasPath&&
            navigator.PathStatus == UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            float remaining = navigator.RemainingDistance;
            if(!float.IsInfinity(remaining)&&
                !float.IsNaN(remaining)&&
                remaining < lastProgressDistance - 0.15f)
            {
                lastProgressDistance = remaining;
                lastProgressTime = Time.time;
            }
        }

        if(!navigator.IsPathPending &&
            navigator.HasPath &&
            navigator.PathStatus == UnityEngine.AI.NavMeshPathStatus.PathComplete&&
            Time.time-lastProgressTime>=6f&&
            Vector3.Distance(transform.position, currentTarget.position) > targetArrivalDistance)
        {
            Debug.LogWarning("Companion takıldı: 6 saniyedir ilerleme yok.");
            navigator.Stop();
            currentTarget = null;
            onInteractionCompleted = null;
            stateMachine.ChangeState(CompanionState.Failed);
            return;
        }

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
            System.Action returnedAction = onReturnedToPlayer;
            onReturnedToPlayer = null;
            returnedAction?.Invoke();
            return;
        }
        navigator.MoveTo(player);
    }

    private void HandleFailed()
    {
        if (failedSince < 0f)
        {
            failedSince = Time.time;
            currentTarget = null;
            onInteractionCompleted = null;
            onReturnedToPlayer = null;
            navigator.Stop();
            return;
        }
        if (Time.time - failedSince < 2f)
        {
            return;
        }
        if (!navigator.IsOnNavMesh)
        {
            return;
        }
        failedSince = -1f;
        Debug.Log("Companion: Başarısız görevden toparlandı.");
        stateMachine.ChangeState(CompanionState.Following);
    }
    public void GoToTarget(Transform target, System.Action onCompleted = null, System.Action onReturned = null)
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
        onReturnedToPlayer = onReturned;
        currentTarget = target;
        noPathSince = -1f;
        lastProgressDistance = float.PositiveInfinity;
        lastProgressTime = Time.time;
        targetStartTime = Time.time;
        stateMachine.ChangeState(CompanionState.GoingToTarget);
        navigator.MoveTo(currentTarget);
    }
}
