
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

    // Diyalog iþ mantýðý ayrý sýnýfta yönetilir.
    private DialogueService dialogueService;

    // Mevcut dýþ eriþim korunuyor.
    // Gerçek kapý bu deðeri doðrudan kullanmamalý.
    public string LastValidatedAction =>
        dialogueService?.LastValidatedAction;

    // Unity Inspector referanslarýný kullanarak
    // diyalog servisini gerektiðinde oluþturur.
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
                "DialoguePanel referansý eksik."
            );
            return;
        }

        // Önceki konuþmanýn izni ve iþlemi temizlenir.
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

    // SendButton OnClick baðlantýsý korunuyor.
    public void OnSendClicked()
    {
        // Ayný anda ikinci mesaj gönderilmesini önle.
        if (dialogueService != null &&
            dialogueService.IsWaitingForResponse)
        {
            return;
        }

        // Yeni mesaj eski çýkýþ iznini geçersiz kýlar.
        dialogueService?.Cancel();

        if (messageInput == null || responseText == null)
        {
            Debug.LogError(
                "Diyalog UI referanslarý eksik."
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

        // Diyalog iþ mantýðý serviste çalýþýr.
        // UI yalnýzca gelen sonucu gösterir.
        StartCoroutine(
            dialogueService.ProcessDialogue(
                userMessage,
                ShowProgress,
                ShowResponse,
                ShowError
            )
        );
    }

    // Kullanýcýya bekleme durumunu gösterir.
    private void ShowProgress(string message)
    {
        if (responseText != null)
            responseText.text = message;
    }

    // Doðrulanmýþ model cevabýný gösterir.
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
    }

    // Baðlantý veya doðrulama hatasýný gösterir.
    private void ShowError(string message)
    {
        if (responseText != null)
            responseText.text = message;
    }

    // Elif'in final kapýsý entegrasyonunda
    // kullanýlacak güvenli izin metodu.
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
