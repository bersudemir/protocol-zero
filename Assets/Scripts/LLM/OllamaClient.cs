
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class OllamaClient : MonoBehaviour, ILLMClient
{
    [SerializeField]
    private string endpoint = "http://localhost:11434/api/chat";

    [SerializeField]
    private string model = "gemma3:4b";

    [SerializeField]
    private int timeoutSeconds = 60;

    private readonly OllamaRequestBuilder requestBuilder =
        new OllamaRequestBuilder();

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
            onError?.Invoke(
                "LLM isteði için gerekli bilgi eksik."
            );
            yield break;
        }

        // Ýstek hazýrlama iþi ayrý sýnýfta.
        string json = requestBuilder.BuildRequestJson(
            model,
            systemPrompt,
            userMessage,
            canOpenExit
        );

        // Að iletiþimini bu sýnýf yönetir.
        yield return SendJsonRequest(
            json,
            onSuccess,
            onError
        );
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
                new UploadHandlerRaw(
                    Encoding.UTF8.GetBytes(json)
                );

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            request.timeout = Mathf.Max(1, timeoutSeconds);

            yield return request.SendWebRequest();

            // Baðlantý veya HTTP hatasý
            if (request.result !=
                UnityWebRequest.Result.Success)
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

            // Sunucunun JSON yanýtýný oku.
            OllamaChatResponse response;

            try
            {
                response =
                    JsonUtility.FromJson<OllamaChatResponse>(
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
                string.IsNullOrWhiteSpace(
                    response.message.content))
            {
                onError?.Invoke(
                    "Modelden geçerli cevap alýnamadý."
                );

                yield break;
            }

            string rawContent = response.message.content;

            Debug.Log(
                "RAW LLM CONTENT: " + rawContent
            );

            onSuccess?.Invoke(rawContent);
        }
    }
}
