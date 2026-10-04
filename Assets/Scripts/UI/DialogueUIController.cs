using TMPro;
using UnityEngine;

public class DialogueUIController : MonoBehaviour
{
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private TMP_Text responseText;
    [SerializeField] private OllamaClient ollamaClient;

    public void OnSendClicked()//butona týklandýðýnda çaðrýlýr
    {
        string userMessage = messageInput.text.Trim();

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            responseText.text = "Lütfen bir mesaj yaz.";
            return;
        }

        responseText.text = "Düþünüyor...";

        string temporarySystemPrompt =
            "Sadece JSON formatýnda cevap ver. " +
            "Cevap tam olarak dialogue ve action alanlarýný içersin. " +
            "action þimdilik her zaman NONE olsun.";

        StartCoroutine(
            ollamaClient.SendChatRequest(
                temporarySystemPrompt,
                userMessage,
                OnOllamaSuccess,
                OnOllamaError
            )
        );
    }

    private void OnOllamaSuccess(string rawContent)
    {
        responseText.text = rawContent;
        Debug.Log("RAW LLM RESPONSE: " + rawContent);
    }

    private void OnOllamaError(string errorMessage)
    {
        responseText.text = errorMessage;
    }
}