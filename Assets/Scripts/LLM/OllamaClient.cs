
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

// Ollama'ya gönderilen mesaj
[Serializable]
public class OllamaMessage
{
    public string role;
    public string content;
}

// JSON Schema alanlarý
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

// Ollama'ya gönderilecek istek
[Serializable]
public class OllamaChatRequest
{
    public string model;
    public OllamaMessage[] messages;
    public bool stream;
    public LLMResponseSchema format;
    public OllamaOptions options;
}

// Ollama'dan gelen sunucu yanýtý
[Serializable]
public class OllamaChatResponse
{
    public OllamaMessage message;
}

public class OllamaClient : MonoBehaviour, ILLMClient
{
    [SerializeField]
    private string endpoint = "http://localhost:11434/api/chat";

    [SerializeField]
    private string model = "gemma3:4b";

    [SerializeField]
    private int timeoutSeconds = 60;

    public IEnumerator SendChatRequest(
    string systemPrompt,
    string userMessage,
    bool canOpenExit,
    Action<string> onSuccess,
    Action<string> onError)
    {
        if (string.IsNullOrWhiteSpace(systemPrompt) ||
            string.IsNullOrWhiteSpace(userMessage))
        {
            onError?.Invoke("LLM isteði için gerekli bilgi eksik.");
            yield break;
        }
        LLMResponseSchema schema = new LLMResponseSchema();

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

        string json = JsonUtility.ToJson(requestData);

        yield return SendJsonRequest(json, onSuccess, onError);
    }

    private IEnumerator SendJsonRequest(
        string json,
        Action<string> onSuccess,
        Action<string> onError)
    {
        using (UnityWebRequest request =
            new UnityWebRequest(
                endpoint,
                UnityWebRequest.kHttpVerbPOST))
        {
            request.uploadHandler =
                new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            request.timeout = Mathf.Max(1, timeoutSeconds);

            yield return request.SendWebRequest();

            // Baðlantý veya HTTP hatasý
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning(
                    "Ollama request failed: " +
                    request.error +
                    " HTTP: " + request.responseCode
                );

                if (request.responseCode == 404)
                {
                    onError?.Invoke(
                        "Ollama modeli veya API adresi bulunamadý."
                    );
                }
                else if (request.result ==
                         UnityWebRequest.Result.ConnectionError)
                {
                    onError?.Invoke(
                        "Ollama sunucusuna baðlanýlamadý veya zaman aþýmý oluþtu."
                    );
                }
                else
                {
                    onError?.Invoke(
                        "Ollama isteði baþarýsýz oldu. HTTP: " +
                        request.responseCode
                    );
                }

                yield break;
            }

            // Sunucunun JSON yanýtýný oku
            OllamaChatResponse response;

            try
            {
                response = JsonUtility.FromJson<OllamaChatResponse>(
                    request.downloadHandler.text
                );
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "Ollama response parse hatasý: " +
                    exception.Message
                );

                onError?.Invoke(
                    "Ollama geçersiz bir sunucu yanýtý döndürdü."
                );

                yield break;
            }

            if (response == null ||
                response.message == null ||
                string.IsNullOrWhiteSpace(response.message.content))
            {
                onError?.Invoke(
                    "Modelden geçerli cevap alýnamadý."
                );

                yield break;
            }

            // Modelin ürettiði JSON içeriðini
            // doðrulama sistemine gönderiyoruz.
            string rawContent = response.message.content;

            Debug.Log("RAW LLM CONTENT: " + rawContent);

            onSuccess?.Invoke(rawContent);
        }
    }
}
