using UnityEngine;

public class KeycardTask : MonoBehaviour
{
    [Header("keycard Görevi")]
    [SerializeField] private CompanionController companion;
    [SerializeField] private GameObject keycard;
    [SerializeField] private Transform holdPoint;
    private CompanionStateMachine stateMachine;
    private bool taskStarted = false;
    private bool isCarrying = false;
    private bool isDelivered = false;
    public bool CanStartTask => !taskStarted && !isDelivered;
    [ContextMenu("TEST - Keycard görevini başlat.")]
    public void BeginTask()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Keycard görevi yalnızca Play modunda başlatılabilir.");
            return;
        }
        if (taskStarted||isDelivered)
        {
            Debug.LogWarning("Keycard görevi zaten başlatılmış veya tamamlanmış.");
            return;
        }
        if (companion==null||keycard==null||holdPoint==null)
        {
            Debug.LogWarning("KeycardTask: Inspector bağlantıları eksik.");
            return;
        }
        stateMachine = companion.GetComponent<CompanionStateMachine>();
        if (stateMachine==null||
            (stateMachine.CurrentState!=CompanionState.Following&&
            stateMachine.CurrentState!=CompanionState.Waiting))
        {
            Debug.LogWarning("Companion şu anda Keycard görevini alamıyor.");
            return;
        }
        taskStarted = true;
        companion.GoToTarget(keycard.transform, TakeKeycard, DeliverKeycard);
        Debug.Log("Keycard görevi Companion'a verildi.");
    }

    private void TakeKeycard()
    {
        if (!taskStarted||isDelivered||keycard==null)
        {
            return;
        }
        Collider cardCollider = keycard.GetComponent<Collider>();
        if (cardCollider!=null)
        {
            cardCollider.enabled = false;
        }
        keycard.transform.SetParent(holdPoint);
        keycard.transform.localPosition = Vector3.zero;
        keycard.transform.localRotation = Quaternion.identity;
        isCarrying = true;
        Debug.Log("Companion Keyacard'ı aldı ve taşımaya başladı.");
    }
    private void DeliverKeycard()
    {
        if (!isCarrying || isDelivered)
        {
            return;
        }
        Collider cardCollider = keycard.GetComponent<Collider>();
        if (cardCollider != null)
        {
            cardCollider.enabled = false;
        }
        isDelivered = true;
        isCarrying = false;
        taskStarted = false;
        keycard.SetActive(false);
        Debug.Log("Companion Keyacard'ı Player'a teslim etti.");
    }
    private void Update()
    {
        if (!taskStarted || isCarrying || stateMachine == null)
        {
            return;
        }
        if (stateMachine.CurrentState == CompanionState.Failed)
        {
            taskStarted = false;
            Debug.LogWarning("Keycard görevi başarısız oldu. Tekrar denenebilir.");
        }
    }
}
