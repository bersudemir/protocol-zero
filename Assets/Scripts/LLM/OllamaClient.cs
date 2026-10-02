using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class OllamaMessage
{
    public string role;
    public string content;
}

[Serializable]
public class StringSchema
{
    public string type = "string";
}

[Serializable]
public class ActionSchema
{
    public string type = "string";
    public string[] @enum = { "NONE" };
}

[Serializable]
public class LLMResponseProperties
{
    public StringSchema dialogue = new StringSchema();
    public ActionSchema action = new ActionSchema();
}

[Serializable]
public class LLMResponseSchema
{
    public string type = "object"; //nesne tipinde
    public LLMResponseProperties properties = new LLMResponseProperties(); //alanlar
    public string[] required = { "dialogue", "action" }; //zorunlu alanlar
    public bool additionalProperties = false; //ekstra alanlara izin verilmez
}

[Serializable]
public class OllamaOptions//modelin davranýþýný kontrol etmek için ayarlar
{
    public float temperature = 0f; //rastgelelik seviyesi
}

[Serializable]
public class OllamaChatRequest
{
    public string model;
    public OllamaMessage[] messages;
    public bool stream;
    public LLMResponseSchema format;
    public OllamaOptions options;
}

[Serializable]
public class OllamaChatResponse
{
    public OllamaMessage message;
}

public class OllamaClient : MonoBehaviour
{
    [SerializeField]
    private string endpoint = "http://localhost:11434/api/chat";

    [SerializeField]
    private string model = "qwen2.5:1.5b";

    public IEnumerator SendChatRequest( 
        string userMessage,
        Action<LLMResponse> onSuccess,
        Action<string> onError)
    {
        OllamaChatRequest requestData = new OllamaChatRequest{
            model = model,

            messages = new[]
    {
        new OllamaMessage
        {
            role = "system",
            content =
                "Sadece JSON formatýnda cevap ver. " +
                "Cevap tam olarak dialogue ve action alanlarýný içersin. " +
                "action þimdilik her zaman NONE olsun."
        },

        new OllamaMessage
        {
            role = "user",
            content = userMessage
        }
    },

            stream = false,
            format = new LLMResponseSchema(),
            options = new OllamaOptions()

        };

        string json = JsonUtility.ToJson(requestData);

        using UnityWebRequest request =
            new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST); //http isteði POST oluþturuldu

        request.uploadHandler =
            new UploadHandlerRaw(Encoding.UTF8.GetBytes(json)); //json verisi yükleme için ayarlandý

        request.downloadHandler =
            new DownloadHandlerBuffer(); //ollama'dan gelen yanýtý almak için 

        request.SetRequestHeader( //istek baþlýðý eklendi
            "Content-Type",
            "application/json"
        );

        yield return request.SendWebRequest(); //istek gönderildi

        if (request.result != UnityWebRequest.Result.Success) //istek baþarýsýzsa hata mesajý döndürülür
        {
            onError?.Invoke(
                "Ollama baðlantý hatasý: " + request.error
            );

            yield break;
        }

        OllamaChatResponse response =
            JsonUtility.FromJson<OllamaChatResponse>(request.downloadHandler.text); //ollama'dan gelen yanýt json formatýna çevrildi

        if (response == null ||
            response.message == null ||
            string.IsNullOrWhiteSpace(response.message.content))
        {
            onError?.Invoke("Modelden geçerli cevap alýnamadý.");
            yield break;
        }

        LLMResponse llmResponse;
        Debug.Log("RAW LLM CONTENT: " + response.message.content);
        try
        {
            llmResponse = JsonUtility.FromJson<LLMResponse>( //ollama'dan gelen yanýt json formatýna çevrildi
                response.message.content 
            );
        }
        catch (Exception)
        {
            onError?.Invoke("Model geçersiz JSON döndürdü.");
            yield break;
        }

        if (llmResponse == null ||
            string.IsNullOrWhiteSpace(llmResponse.dialogue) ||
            string.IsNullOrWhiteSpace(llmResponse.action))
        {
            onError?.Invoke("Model cevabýnda gerekli alanlar eksik.");
            yield break;
        }

        onSuccess?.Invoke(llmResponse);
    }
}