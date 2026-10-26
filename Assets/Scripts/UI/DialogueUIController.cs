
using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueUIController : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField] private GameState gameState;
    [SerializeField] private OllamaClient ollamaClient;
    [SerializeField] private NPCProfile npcProfile;

    [Header("UI References")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private TMP_Text responseText;
    [SerializeField] private GameObject dialoguePanel;

    [Header("Door Connection")]
    [SerializeField] private MuratExitDoorBridge muratDoorLink;

    private DialogueService dialogueService;

    public string LastValidatedAction =>
        dialogueService?.LastValidatedAction;

    private bool TryInitializeService()
    {
        if (dialogueService != null)
            return true;

        if (gameState == null ||
            ollamaClient == null ||
            npcProfile == null)
        {
            return false;
        }

        dialogueService = new DialogueService(
            ollamaClient,
            gameState,
            npcProfile
        );

        return true;
    }

    [ContextMenu("TEST - Open Dialogue")]
    public void OpenDialogue()
    {
        if (dialoguePanel == null)
        {
            Debug.LogWarning(
                "DialoguePanel referansi eksik."
            );
            return;
        }

        StopAllCoroutines();
        dialogueService?.Cancel();

        dialoguePanel.SetActive(true);
    }

    [ContextMenu("TEST - Close Dialogue")]
    public void CloseDialogue()
    {
        StopAllCoroutines();
        dialogueService?.Cancel();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void OnSendClicked()
    {
        if (dialogueService != null &&
            dialogueService.IsWaitingForResponse)
        {
            return;
        }

        dialogueService?.Cancel();

        if (messageInput == null || responseText == null)
        {
            Debug.LogError(
                "Diyalog UI referanslari eksik."
            );
            return;
        }

        if (!TryInitializeService())
        {
            responseText.text = "LLM baðlantýlarý eksik.";
            return;
        }

        string userMessage =
            messageInput.text?.Trim();

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            responseText.text = "Lütfen bir mesaj yaz.";
            return;
        }

        if (!dialogueService.TryBeginDialogue())
            return;

        StartCoroutine(
            dialogueService.ProcessDialogue(
                userMessage,
                ShowProgress,
                ShowResponse,
                ShowError
            )
        );
    }

    private void ShowProgress(string message)
    {
        if (responseText != null)
            responseText.text = message;
    }

    private void ShowResponse(LLMResponse response)
    {
        if (response == null)
            return;

        if (responseText != null)
            responseText.text = response.dialogue;

        Debug.Log(
            "LLM INTENT: " + response.intent +
            " | ACTION: " + response.action
        );

        // Yalnizca dogrulanmis OPEN_EXIT
        // kararinda kapi baglantisini cagir.
        if (response.action == "OPEN_EXIT")
        {
            if (muratDoorLink != null)
            {
                muratDoorLink.TryOpenExit();
            }
            else
            {
                Debug.LogWarning(
                    "MuratDoorLink referansi eksik."
                );
            }
        }
    }

    private void ShowError(string message)
    {
        if (responseText != null)
            responseText.text = message;
    }

    public bool TryConsumeExitPermission()
    {
        return dialogueService != null &&
               dialogueService.TryConsumeExitPermission();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        dialogueService?.Cancel();
    }
}
