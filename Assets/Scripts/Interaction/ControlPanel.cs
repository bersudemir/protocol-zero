using Unity.VisualScripting;
using UnityEngine;

public class ControlPanel : MonoBehaviour, IInteractable,IInteractionPromptProvider
{
    public string InteractionPrompt => "[E] Companion'a kapıyı açtır.";
    [SerializeField] private CompanionController companion;
    [SerializeField] private Transform companionTarget;
    [SerializeField] private DoorController door;
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
        if (door == null)
        {
            Debug.LogWarning("ControlPanel: Kapı atanmadı.");
            return;
        }
        companion.GoToTarget(companionTarget, () =>
        {
            door.OpenDoor();
            enabled = false;
        });
        Debug.Log("ControlPanel görevi Companiona'a verildi.");
    }
}
