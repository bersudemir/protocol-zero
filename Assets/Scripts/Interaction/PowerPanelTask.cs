using UnityEngine;
[RequireComponent(typeof(Collider))]
public class PowerPanelTask : MonoBehaviour,IInteractable
{
    [Header("PowerPanel Görevi")]
    [SerializeField] private CompanionController companion;
    [SerializeField] private Transform companionTarget;
    [SerializeField] private Renderer panelRenderer;

    [Header("Güç Açık Rengi")]
    [SerializeField] private Color poweredColor = Color.green;
    private CompanionStateMachine stateMachine;
    private Collider panelCollider;
    private bool taskStarted = false;
    private bool isPowerRestored = false;
    public bool PowerRestored => isPowerRestored;
    public bool CanStartTask => !taskStarted && !isPowerRestored;
    private void Awake()
    {
        panelCollider = GetComponent<Collider>();
        if (panelRenderer == null)
        {
            panelRenderer = GetComponent<Renderer>();
        }
    }
    public void Interact()
    {
        if (!CanStartTask)
        {
            return;
        }
        if (companion==null || companionTarget==null)
        {
            Debug.LogWarning("PowerPanel: Inspector bağlantıları eksik.");
            return;
        }
        stateMachine = companion.GetComponent<CompanionStateMachine>();
        if (stateMachine == null ||
           (stateMachine.CurrentState != CompanionState.Following &&
           stateMachine.CurrentState != CompanionState.Waiting))
        {
            Debug.LogWarning("Companion şu anda PowerPanel görevini alamıyor.");
            return;
        }
        taskStarted = true;
        panelCollider.enabled = false;
        companion.GoToTarget(companionTarget, ActivatePower);
        Debug.Log("PowerPanel görevi Companion'a verildi.");
    }

    private void ActivatePower()
    {
        if (!taskStarted || isPowerRestored)
        {
            return;
        }
        isPowerRestored = true;
        taskStarted = false;
        if (panelRenderer != null)
        {
            panelRenderer.material.color = poweredColor;
        }
        Debug.Log("PowerPanel: Elektrik etkinleştirildi.");
    }
    private void Update()
    {
        if (!taskStarted || stateMachine==null)
        {
            return;
        }
        if (stateMachine.CurrentState == CompanionState.Failed)
        {
            taskStarted = false;
            panelCollider.enabled = true;
            Debug.LogWarning("PowerPanel görevi başarısız oldu. Tekrar denenebilir.");
        }
    }
}
