
using System;

public class LLMResponseValidator
{
    private readonly LLMResponseParser responseParser;
    private readonly LLMActionExecutor actionExecutor;
    private readonly LLMDialogueValidator dialogueValidator;

    // Varsayýlan kullaným:
    // Üç doðrulayýcýyý oluþturur.
    public LLMResponseValidator()
        : this(
            new LLMResponseParser(),
            new LLMActionExecutor(),
            new LLMDialogueValidator())
    {
    }

    // Önceki iki parametreli constructor
    // geriye dönük uyumluluk için korunur.
    public LLMResponseValidator(
        LLMResponseParser responseParser,
        LLMActionExecutor actionExecutor)
        : this(
            responseParser,
            actionExecutor,
            new LLMDialogueValidator())
    {
    }

    // Baðýmlýlýklar dýþarýdan verilebilir.
    // Bu sayede her bileþen ayrý test edilebilir.
    public LLMResponseValidator(
        LLMResponseParser responseParser,
        LLMActionExecutor actionExecutor,
        LLMDialogueValidator dialogueValidator)
    {
        this.responseParser = responseParser
            ?? throw new ArgumentNullException(
                nameof(responseParser));

        this.actionExecutor = actionExecutor
            ?? throw new ArgumentNullException(
                nameof(actionExecutor));

        this.dialogueValidator = dialogueValidator
            ?? throw new ArgumentNullException(
                nameof(dialogueValidator));
    }

    // LLM yanýtýnýn bütün doðrulama
    // aþamalarýný tek merkezden yönetir.
    public bool TryValidate(
        string rawContent,
        GameState gameState,
        out LLMResponse validatedResponse,
        out string errorMessage)
    {
        validatedResponse = null;
        errorMessage = null;

        // 1. Gerçek Unity durumu zorunlu.
        if (gameState == null)
        {
            errorMessage = "GameState bulunamadý.";
            return false;
        }

        // 2. JSON ve alan doðrulamasý.
        bool parsed = responseParser.TryParse(
            rawContent,
            out LLMResponse parsedResponse,
            out errorMessage
        );

        if (!parsed || parsedResponse == null)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
            {
                errorMessage =
                    "LLM yanýtý ayrýþtýrýlamadý.";
            }

            return false;
        }

        // 3. Modelin ürettiði intent'i
        // PlayerIntent enum deðerine dönüþtür.
        bool intentParsed = Enum.TryParse(
            parsedResponse.intent,
            false,
            out PlayerIntent playerIntent
        );

        if (!intentParsed ||
            playerIntent == PlayerIntent.Unknown ||
            !Enum.IsDefined(
                typeof(PlayerIntent),
                playerIntent))
        {
            errorMessage =
                "LLM geçersiz oyuncu niyeti üretti.";

            return false;
        }

        // 4. Action kararý ile gerçek
        // Unity oyun durumunu karþýlaþtýr.
        bool actionValid = actionExecutor.CanApplyAction(
            parsedResponse.action,
            playerIntent,
            gameState,
            out string actionFeedback
        );

        if (!actionValid)
        {
            errorMessage =
                string.IsNullOrWhiteSpace(actionFeedback)
                    ? "LLM action kararý doðrulanamadý."
                    : actionFeedback;

            return false;
        }

        // 5. Konuþmanýn action ve gerçek
        // Unity bilgileriyle çeliþip
        // çeliþmediðini kontrol et.
        bool dialogueValid = dialogueValidator.TryValidate(
            parsedResponse,
            gameState.HasKeycard,
            gameState.PowerRestored,
            out string dialogueFeedback
        );

        if (!dialogueValid)
        {
            errorMessage =
                string.IsNullOrWhiteSpace(dialogueFeedback)
                    ? "LLM konuþmasý doðrulanamadý."
                    : dialogueFeedback;

            return false;
        }

        // 6. Bütün mevcut kontroller geçti.
        // Modelin yanýtýný deðiþtirmeden döndür.
        validatedResponse = parsedResponse;

        return true;
    }
}
