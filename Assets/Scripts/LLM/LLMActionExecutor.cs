
using System;

public enum PlayerIntent
{
    Unknown,
    EvacuationRequest,
    ThreatOrSabotage,
    OtherOrUnclear
}

public class LLMActionExecutor
{
    public bool CanApplyAction(
        string llmAction,
        PlayerIntent playerIntent,
        GameState gameState,
        out string validationFeedback)
    {
        validationFeedback = null;

        if (gameState == null)
        {
            validationFeedback = "GameState bulunamadý.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(llmAction))
        {
            validationFeedback = "Action boþ geldi.";
            return false;
        }

        string expectedAction;

        switch (playerIntent)
        {
            case PlayerIntent.ThreatOrSabotage:
                expectedAction = "DENY";
                break;

            case PlayerIntent.OtherOrUnclear:
                expectedAction = "REQUEST_MORE_INFO";
                break;

            case PlayerIntent.EvacuationRequest:
                expectedAction =
                    gameState.HasKeycard && gameState.PowerRestored
                        ? "OPEN_EXIT"
                        : "DENY";
                break;

            default:
                validationFeedback = "Geçersiz oyuncu niyeti.";
                return false;
        }

        if (llmAction.Trim() != expectedAction)
        {
            validationFeedback =
                "Modelin kararý oyun kurallarýyla uyuþmuyor. " +
                "Beklenen action: " + expectedAction;

            return false;
        }

        return true;
    }
}
