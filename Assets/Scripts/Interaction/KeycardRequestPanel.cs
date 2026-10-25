using UnityEngine;

public class KeycardRequestPanel : MonoBehaviour, IInteractable
{
    [SerializeField] private KeycardTask keycardTask;
    public void Interact()
    {
        if (keycardTask == null)
        {
            Debug.LogWarning("KeycardRequestPanel: KeycardTask bağlantısı eksik.");
            return;
        }
        keycardTask.BeginTask();
    }
    private Collider panelCollider;
    private void Awake()
    {
        panelCollider = GetComponent<Collider>();
    }
    private void Update()
    {
        if (panelCollider == null || keycardTask == null)
            return;
        panelCollider.enabled = keycardTask.CanStartTask;
    }
}
