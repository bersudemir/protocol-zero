
using System;

public class LLMResponseValidator
{
    private readonly LLMResponseParser responseParser;
    private readonly LLMActionPolicy actionPolicy;

    public LLMResponseValidator()
        : this(
            new LLMResponseParser(),
            new LLMActionPolicy())
    {
    }

    public LLMResponseValidator(
        LLMResponseParser responseParser,
        LLMActionPolicy actionPolicy)
    {
        this.responseParser = responseParser
            ?? throw new ArgumentNullException(
                nameof(responseParser));

        this.actionPolicy = actionPolicy
            ?? throw new ArgumentNullException(
                nameof(actionPolicy));
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

        // 1. Gelen JSON'u ayrýþtýr.
        if (!responseParser.TryParse(
            rawContent,
            out LLMResponse parsedResponse,
            out errorMessage) || parsedResponse == null)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                errorMessage = "LLM yanýtý ayrýþtýrýlamadý.";

            return false;
        }

        // 2. Oyuncunun niyetini kontrol et.
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

        // 3. JSON action deðerini enum'a dönüþtür.
        if (!LLMActionJson.TryParse(
            parsedResponse.action,
            out LLMAction action))
        {
            errorMessage = "Geçersiz action.";
            return false;
        }

        // 4. Eylemi gerçek GameState ile doðrula.
        if (!actionPolicy.CanApplyAction(
            action,
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
