using UnityEngine;

public class ControlPanel : MonoBehaviour, IInteractable
{
    [SerializeField] private CompanionController companion;
    [SerializeField] private Transform companionTarget;
    public void Interact()
    {
        if (companion == null)
        {
            Debug.LogWarning("ControlPanel: Companion atanmadı.");
            return;
        }
        if (companionTarget == null)
        {
            Debug.LogWarning("ControlPanel: Companion hedefi atanmadı.");
            return;
        }
        companion.GoToTarget(companionTarget);
        Debug.Log("ControlPanel görevi Companiona'a verildi.");
    }
}
