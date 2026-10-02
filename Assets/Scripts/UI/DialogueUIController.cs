using TMPro;
using UnityEngine;

public class DialogueUIController : MonoBehaviour
{
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private TMP_Text responseText;
    [SerializeField] private OllamaClient ollamaClient;

    public void OnSendClicked()
    {
        string userMessage = messageInput.text.Trim();

        if (string.IsNullOrWhiteSpace(userMessage))
        {
            responseText.text = "Lütfen bir mesaj yaz.";
            return;
        }
        responseText.text = "Düþünüyor...";

        StartCoroutine(
            ollamaClient.SendChatRequest(userMessage,OnOllamaSuccess,OnOllamaError)
        );
    }

    private void OnOllamaSuccess(LLMResponse modelResponse)
    {
        responseText.text = modelResponse.dialogue;

        Debug.Log("LLM Action: " + modelResponse.action);
    }

    private void OnOllamaError(string errorMessage)
    {
        responseText.text = errorMessage;
    }
}