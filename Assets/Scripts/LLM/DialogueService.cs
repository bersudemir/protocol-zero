
using System;
using System.Collections;

public class DialogueService
{
    private readonly ILLMClient llmClient;
    private readonly GameState gameState;
    private readonly NPCProfile npcProfile;

    private readonly PromptBuilder promptBuilder =
        new PromptBuilder();

    private readonly LLMResponseValidator responseValidator =
        new LLMResponseValidator();

    private const int MaxRetries = 1;

    private bool isWaitingForResponse;
    private LLMAction lastValidatedAction = LLMAction.Unknown;
    private int lastValidatedStateVersion = -1;

    public bool IsWaitingForResponse => isWaitingForResponse;

    public string LastValidatedAction =>
        LLMActionJson.ToJsonValue(lastValidatedAction);

    public DialogueService(
        ILLMClient llmClient,
        GameState gameState,
        NPCProfile npcProfile)
    {
        this.llmClient = llmClient
            ?? throw new ArgumentNullException(nameof(llmClient));

        this.gameState = gameState
            ?? throw new ArgumentNullException(nameof(gameState));

        this.npcProfile = npcProfile
            ?? throw new ArgumentNullException(nameof(npcProfile));
    }

    // Yeni konuþmayý baþlatýr.
    // Ayný anda iki istek gönderilmesini önler.
    public bool TryBeginDialogue()
    {
        if (isWaitingForResponse)
            return false;

        ClearValidatedAction();
        isWaitingForResponse = true;
        return true;
    }

    public IEnumerator ProcessDialogue(
        string userMessage,
        Action<string> onProgress,
        Action<LLMResponse> onSuccess,
        Action<string> onError)
    {
        bool hasKeycard = gameState.HasKeycard;
        bool powerRestored = gameState.PowerRestored;
        int stateVersion = gameState.StateVersion;

        string lastFeedback = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (!IsGameStateCurrent(
                hasKeycard, powerRestored, stateVersion))
            {
                Fail("Oyun durumu deðiþti. Tekrar deneyin.", onError);
                yield break;
            }

            onProgress?.Invoke(
                attempt == 0
                    ? "Murat düþünüyor..."
                    : "Murat yeniden deðerlendiriyor..."
            );

            string systemPrompt = attempt == 0
                ? promptBuilder.BuildSystemPrompt(
                    npcProfile,
                    hasKeycard,
                    powerRestored)
                : promptBuilder.BuildReviewPrompt(
                    npcProfile,
                    hasKeycard,
                    powerRestored,
                    lastFeedback);

            string rawResponse = null;
            string requestError = null;

            // Gerçek LLM isteði.
            // HTTP ayrýntýlarýný DialogueService bilmez.
            yield return llmClient.SendChatRequest(
                systemPrompt,
                userMessage,
                hasKeycard && powerRestored,
                result => rawResponse = result,
                error => requestError = error
            );

            // Yanýt beklenirken oyun durumu deðiþmiþ olabilir.
            if (!IsGameStateCurrent(
                hasKeycard, powerRestored, stateVersion))
            {
                Fail("Oyun durumu deðiþti. Tekrar deneyin.", onError);
                yield break;
            }

            if (!string.IsNullOrEmpty(requestError))
            {
                Fail(requestError, onError);
                yield break;
            }

            bool valid = responseValidator.TryValidate(
                rawResponse,
                gameState,
                out LLMResponse response,
                out lastFeedback
            );

            if (valid)
            {
                // Validator'dan geçmiþ action'ý
                // Unity içinde enum olarak saklarýz.
                if (!LLMActionJson.TryParse(
                    response.action,
                    out LLMAction action))
                {
                    Fail("Geçersiz action.", onError);
                    yield break;
                }

                lastValidatedAction = action;

                lastValidatedStateVersion =
                    action == LLMAction.OpenExit
                        ? stateVersion
                        : -1;

                isWaitingForResponse = false;
                onSuccess?.Invoke(response);
                yield break;
            }
        }

        Fail(
            "Murat'ýn yanýtý doðrulanamadý. Tekrar deneyin.",
            onError
        );
    }

    // Eski veya tekrar kullanýlan çýkýþ izinlerini reddeder.
    public bool TryConsumeExitPermission()
    {
        bool allowed =
            !isWaitingForResponse &&
            lastValidatedAction == LLMAction.OpenExit &&
            gameState != null &&
            gameState.HasKeycard &&
            gameState.PowerRestored &&
            gameState.StateVersion == lastValidatedStateVersion;

        ClearValidatedAction();

        return allowed;
    }

    // Panel kapatýldýðýnda veya istek iptal edildiðinde.
    public void Cancel()
    {
        isWaitingForResponse = false;
        ClearValidatedAction();
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

    private void ClearValidatedAction()
    {
        lastValidatedAction = LLMAction.Unknown;
        lastValidatedStateVersion = -1;
    }

    private void Fail(
        string message,
        Action<string> onError)
    {
        Cancel();
        onError?.Invoke(message);
    }
}
