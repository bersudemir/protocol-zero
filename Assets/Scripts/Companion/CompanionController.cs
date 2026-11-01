using System.Collections;
using UnityEngine;
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
    private CompanionSensor sensor;
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
        sensor = GetComponent<CompanionSensor>();
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
            FailTask("Companion: Görev hedefi bulunamadı.");
            return;
        }
        if(Time.time - targetStartTime >= targetTimeoutSeconds)
        {
            FailTask("Companion: Hedefe ulaşma süresi aşıldı.");
            return;
        }
        if(!navigator.TryMoveTo(currentTarget,out string navigationError))
        {
            FailTask("Companion navigasyon hatası: " + navigationError);
            return;
        }

        if(!navigator.IsPathPending&&
            navigator.HasPath&&
            navigator.PathStatus != UnityEngine.AI.NavMeshPathStatus.PathComplete)
        {
            FailTask("Companion geçersiz veya eksik rota: " + navigator.PathStatus);
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
                FailTask("Companion: Hedefe giden yolda NavMesh yolu bulunamadı.");
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
            string sensorStatus=sensor==null
                ?"Sensör bulunamadı."
                :$"Ön: {sensor.ForwardBlocked}, " +
                $"Sol: {sensor.LeftBlocked}, " +
                $"Sağ: {sensor.RightBlocked}";
            FailTask($"Companion takıldı: 6 saniyedir ilerleme yok. {sensorStatus}");
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
        targetStartTime = Time.time;
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
        if (Time.time - targetStartTime >= targetTimeoutSeconds)
        {
            FailTask("Companion: oyuncuya dönüş süresi aşıldı.");
            return;
        }
        if(!navigator.TryMoveTo(player,out string navigationError))
        {
            FailTask("Companion geri dönemedi." + navigationError);
            return;
        }
    }

    private void FailTask(string reason)
    {
        Debug.LogWarning(reason);
        navigator.Stop();
        currentTarget = null;
        onInteractionCompleted = null;
        onReturnedToPlayer = null;
        noPathSince = -1f;
        stateMachine.ChangeState(CompanionState.Failed);
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
            FailTask("Companion: Gidilecek görev hedefi atanmadı.");
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
