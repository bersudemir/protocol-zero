
public enum LLMAction
{
    Unknown = 0,
    OpenExit = 1,
    Deny = 2,
    RequestMoreInfo = 3
}

// Ollama JSON deðerleri ile C# enum deðerleri
// arasýnda güvenli dönüþüm saðlar.
public static class LLMActionJson
{
    public static bool TryParse(
        string rawAction,
        out LLMAction action)
    {
        action = LLMAction.Unknown;

        switch (rawAction)
        {
            case "OPEN_EXIT":
                action = LLMAction.OpenExit;
                return true;

            case "DENY":
                action = LLMAction.Deny;
                return true;

            case "REQUEST_MORE_INFO":
                action = LLMAction.RequestMoreInfo;
                return true;

            default:
                return false;
        }
    }

    public static string ToJsonValue(LLMAction action)
    {
        switch (action)
        {
            case LLMAction.OpenExit:
                return "OPEN_EXIT";

            case LLMAction.Deny:
                return "DENY";

            case LLMAction.RequestMoreInfo:
                return "REQUEST_MORE_INFO";

            default:
                return null;
        }
    }
}
