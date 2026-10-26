
using UnityEngine;

public class OllamaRequestBuilder
{
    public string BuildRequestJson(
        string model,
        string systemPrompt,
        string userMessage,
        bool canOpenExit)
    {
        LLMResponseSchema schema = new LLMResponseSchema();

        // Kart veya elektrik eksikse modelden
        // OPEN_EXIT üretmesi istenmez.
        if (!canOpenExit)
        {
            schema.properties.action.@enum = new[]
            {
                "DENY",
                "REQUEST_MORE_INFO"
            };
        }

        OllamaChatRequest requestData = new OllamaChatRequest
        {
            model = model,

            messages = new[]
            {
                new OllamaMessage
                {
                    role = "system",
                    content = systemPrompt
                },
                new OllamaMessage
                {
                    role = "user",
                    content = userMessage
                }
            },

            stream = false,
            format = schema,
            options = new OllamaOptions()
        };

        return JsonUtility.ToJson(requestData);
    }
}
