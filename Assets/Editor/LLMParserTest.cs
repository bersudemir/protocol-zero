
using UnityEditor;
using UnityEngine;

public static class LLMParserTest
{
    [MenuItem("Tools/Protocol Zero/Test Invalid JSON")]
    public static void RunTest()
    {
        LLMResponseParser parser = new LLMResponseParser();

        string invalidJson =
            "{\"intent\":\"EvacuationRequest\",";

        bool accepted = parser.TryParse(
            invalidJson,
            out LLMResponse response,
            out string error
        );

        if (!accepted && response == null)
        {
            Debug.Log(
                "[TEST BAŞARILI] Geçersiz JSON reddedildi. " +
                error
            );
        }
        else
        {
            Debug.LogError(
                "[TEST BAŞARISIZ] Geçersiz JSON kabul edildi!"
            );
        }
    }
}
