
using System;

public class LLMResponseValidator
{
    private readonly LLMResponseParser responseParser;
    private readonly LLMActionExecutor actionExecutor;

    public LLMResponseValidator()
        : this(
            new LLMResponseParser(),
            new LLMActionExecutor())
    {
    }

    public LLMResponseValidator(
        LLMResponseParser responseParser,
        LLMActionExecutor actionExecutor)
    {
        this.responseParser = responseParser
            ?? throw new ArgumentNullException(nameof(responseParser));

        this.actionExecutor = actionExecutor
            ?? throw new ArgumentNullException(nameof(actionExecutor));
    }

    public bool TryValidate(
        string rawContent,
        GameState gameState,
        out LLMResponse validatedResponse,
        out string errorMessage)
    {
        validatedResponse = null;
        errorMessage = null;

        if (gameState == null)
        {
            errorMessage = "GameState bulunamadý.";
            return false;
        }

        // 1. JSON kontrolü
        if (!responseParser.TryParse(
            rawContent,
            out LLMResponse parsedResponse,
            out errorMessage) || parsedResponse == null)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                errorMessage = "LLM yanýtý ayrýþtýrýlamadý.";

            return false;
        }

        // 2. Intent kontrolü
        if (!Enum.TryParse(
            parsedResponse.intent,
            false,
            out PlayerIntent playerIntent) ||
            playerIntent == PlayerIntent.Unknown ||
            !Enum.IsDefined(typeof(PlayerIntent), playerIntent))
        {
            errorMessage = "Geçersiz oyuncu niyeti.";
            return false;
        }

        // 3. Action ve gerçek oyun durumu kontrolü
        if (!actionExecutor.CanApplyAction(
            parsedResponse.action,
            playerIntent,
            gameState,
            out string feedback))
        {
            errorMessage = string.IsNullOrWhiteSpace(feedback)
                ? "LLM kararý doðrulanamadý."
                : feedback;

            return false;
        }

        validatedResponse = parsedResponse;
        return true;
    }
}
