
public enum PlayerIntent
{
    Unknown,
    EvacuationRequest,
    ThreatOrSabotage,
    OtherOrUnclear
}

public class LLMActionPolicy
{
    public bool CanApplyAction(
        LLMAction action,
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

        if (action == LLMAction.Unknown)
        {
            validationFeedback = "Geçersiz action.";
            return false;
        }

        LLMAction expectedAction;

        switch (playerIntent)
        {
            case PlayerIntent.ThreatOrSabotage:
                expectedAction = LLMAction.Deny;
                break;

            case PlayerIntent.OtherOrUnclear:
                expectedAction = LLMAction.RequestMoreInfo;
                break;

            case PlayerIntent.EvacuationRequest:
                expectedAction =
                    gameState.HasKeycard &&
                    gameState.PowerRestored
                        ? LLMAction.OpenExit
                        : LLMAction.Deny;
                break;

            default:
                validationFeedback =
                    "Geçersiz oyuncu niyeti.";
                return false;
        }

        if (action != expectedAction)
        {
            validationFeedback =
                "Modelin kararý oyun kurallarýyla uyuþmuyor. " +
                "Beklenen action: " +
                LLMActionJson.ToJsonValue(expectedAction);

            return false;
        }

        return true;
    }
}
