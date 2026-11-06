
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MuratDialogueInteraction : MonoBehaviour,
    IInteractable, IInteractionPromptProvider
{
    [SerializeField]
    private DialogueUIController dialogueUI;

    public string InteractionPrompt => "[E] Murat ile konuş";

    public void Interact()
    {
        if (dialogueUI == null)
        {
            Debug.LogWarning("Murat: Diyalog paneli atanmamış.");
            return;
        }

        dialogueUI.OpenDialogue();
    }
}
