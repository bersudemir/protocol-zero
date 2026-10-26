
using System;

// Ollama mesaj veri modeli
[Serializable]
public class OllamaMessage
{
    public string role;
    public string content;
}

// JSON Schema veri modelleri
[Serializable]
public class StringSchema
{
    public string type = "string";
}

[Serializable]
public class IntentValueSchema
{
    public string type = "string";

    public string[] @enum =
    {
        "EvacuationRequest",
        "ThreatOrSabotage",
        "OtherOrUnclear"
    };
}

[Serializable]
public class ActionSchema
{
    public string type = "string";

    public string[] @enum =
    {
        "OPEN_EXIT",
        "REQUEST_MORE_INFO",
        "DENY"
    };
}

[Serializable]
public class LLMResponseProperties
{
    public IntentValueSchema intent = new IntentValueSchema();
    public ActionSchema action = new ActionSchema();
    public StringSchema dialogue = new StringSchema();
}

[Serializable]
public class LLMResponseSchema
{
    public string type = "object";

    public LLMResponseProperties properties =
        new LLMResponseProperties();

    public string[] required =
    {
        "intent",
        "action",
        "dialogue"
    };

    public bool additionalProperties = false;
}

[Serializable]
public class OllamaOptions
{
    public float temperature = 0f;
}

// Ollama HTTP isteðinin veri modeli
[Serializable]
public class OllamaChatRequest
{
    public string model;
    public OllamaMessage[] messages;
    public bool stream;
    public LLMResponseSchema format;
    public OllamaOptions options;
}

// Ollama sunucusunun yanýt veri modeli
[Serializable]
public class OllamaChatResponse
{
    public OllamaMessage message;
}
