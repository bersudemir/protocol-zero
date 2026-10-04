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
public class LLMResponseSchema //gelen json verisinin þemasýný tanýmlar
{
    public string type = "object";
    public LLMResponseProperties properties = new LLMResponseProperties();
    public string[] required = { "dialogue", "action" };//alanlarýn zorunlu olduðunu belirtir
    public bool additionalProperties = false; //ekstra alanlarýn kabul edilmediðini belirtir
}

[Serializable]
public class OllamaOptions//modelin davranýþýný kontrol etmek için kullanýlan seçenekler
{
    public float temperature = 0f;//düþükse tutaarlý yüksekse rastgele cevaplar verir
}

[Serializable]
public class OllamaChatRequest
{
    public string model;
    public OllamaMessage[] messages;
    public bool stream; //cevaplarý tekte almak için false
    public LLMResponseSchema format;
    public OllamaOptions options;
}

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
    private string model = "qwen2.5:1.5b";

    public IEnumerator SendChatRequest( //coroutine ile asenkron olarak çalýþýr bu yüzden IEnumerator döndürür
        string systemPrompt, //modele kim olduðunu ve kurallarý verir
        string userMessage,
        Action<string> onSuccess,
        Action<string> onError)
    {
        OllamaChatRequest requestData = new OllamaChatRequest//ollama api'sine gönderilecek veriyi oluþturur
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
            format = new LLMResponseSchema(),
            options = new OllamaOptions()
        };

        string json = JsonUtility.ToJson(requestData);

        using UnityWebRequest request =
            new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPOST);//http post isteði oluþturur

        request.uploadHandler =
            new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));

        request.downloadHandler =
            new DownloadHandlerBuffer();//gelen cevabý tutar

        request.SetRequestHeader(//ollamaya json veri gönderileceðini söyler
            "Content-Type",
            "application/json"
        );

        yield return request.SendWebRequest();//istek gönderilir (unity coroutine)

        if (request.result != UnityWebRequest.Result.Success)//http hata kontrolü
        {
            onError?.Invoke("Ollama baðlantý hatasý: " + request.error);
            yield break;
        }

        OllamaChatResponse response =JsonUtility.FromJson<OllamaChatResponse>(request.downloadHandler.text);

        if (response == null || response.message == null ||string.IsNullOrWhiteSpace(response.message.content))
        {
            onError?.Invoke("Modelden geçerli cevap alýnamadý.");
            yield break;
        }

        string rawContent = response.message.content;//modelden gelen cevabý alýr

        Debug.Log("RAW LLM CONTENT: " + rawContent);

        onSuccess?.Invoke(rawContent);//cevabý geri döndürür
    }
}