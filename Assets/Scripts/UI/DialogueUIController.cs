
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

    private readonly PromptBuilder promptBuilder = new PromptBuilder();
    private readonly LLMResponseValidator responseValidator =
        new LLMResponseValidator();

    private const int MaxRetries = 1;
    private bool isWaitingForResponse;

    // Fiziksel kapýyý tek baþýna açmaz.
    public string LastValidatedAction { get; private set; }

    public void OnSendClicked()
    {
        if (isWaitingForResponse)
            return;

        LastValidatedAction = null;

        if (messageInput == null || responseText == null)
        {
            Debug.LogError("Diyalog UI referanslarý eksik.");
            return;
        }

        if (gameState == null ||
            ollamaClient == null ||
            npcProfile == null)
        {
            responseText.text = "LLM baðlantýlarý eksik.";
            return;
        }

        string userMessage = messageInput.text.Trim();

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            responseText.text = "Lütfen bir mesaj yaz.";
            return;
        }

        isWaitingForResponse = true;

        StartCoroutine(ProcessDialogue(
            userMessage,
            gameState.HasKeycard,
            gameState.PowerRestored,
            gameState.StateVersion
        ));
    }

    private IEnumerator ProcessDialogue(
        string userMessage,
        bool hasKeycard,
        bool powerRestored,
        int stateVersion)
    {
        string lastFeedback = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (!IsGameStateCurrent(
                hasKeycard, powerRestored, stateVersion))
            {
                ShowError("Oyun durumu deðiþti. Tekrar deneyin.");
                yield break;
            }

            responseText.text = attempt == 0
                ? "Murat düþünüyor..."
                : "Murat yeniden deðerlendiriyor...";

            string systemPrompt;

            if (attempt == 0)
            {
                systemPrompt = promptBuilder.BuildSystemPrompt(
                    npcProfile,
                    hasKeycard,
                    powerRestored
                );
            }
            else
            {
                systemPrompt = promptBuilder.BuildReviewPrompt(npcProfile, hasKeycard,powerRestored,lastFeedback);
            }

            string rawResponse = null;
            string requestError = null;

            // Ollama yanýtýný bekle.
            yield return ollamaClient.SendChatRequest(
     systemPrompt,
     userMessage,
     hasKeycard && powerRestored,
     result => rawResponse = result,
     error => requestError = error
 );

            // Yanýt gelene kadar oyun deðiþmiþ olabilir.
            if (!IsGameStateCurrent(
                hasKeycard, powerRestored, stateVersion))
            {
                ShowError("Oyun durumu deðiþti. Tekrar deneyin.");
                yield break;
            }

            if (!string.IsNullOrEmpty(requestError))
            {
                ShowError(requestError);
                yield break;
            }

            // JSON, intent, action ve GameState kontrolü.
            bool valid = responseValidator.TryValidate(
                rawResponse,
                gameState,
                out LLMResponse response,
                out lastFeedback
            );

            if (valid)
            {
                LastValidatedAction = response.action;
                responseText.text = response.dialogue;

                Debug.Log(
                    "LLM INTENT: " + response.intent +
                    " | ACTION: " + response.action
                );

                isWaitingForResponse = false;
                yield break;
            }

            Debug.LogWarning(
                "LLM yanýtý doðrulanamadý: " + lastFeedback
            );
        }

        ShowError(
            "Murat'ýn yanýtý doðrulanamadý. Tekrar deneyin."
        );
    }

    private bool IsGameStateCurrent(
        bool hasKeycard,
        bool powerRestored,
        int stateVersion)
    {
        return gameState != null &&
               gameState.HasKeycard == hasKeycard &&
               gameState.PowerRestored == powerRestored &&
               gameState.StateVersion == stateVersion;
    }

    private void ShowError(string message)
    {
        LastValidatedAction = null;
        isWaitingForResponse = false;

        if (responseText != null)
            responseText.text = message;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isWaitingForResponse = false;
        LastValidatedAction = null;
    }
}
