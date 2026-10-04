using System;
using System.Collections;

public interface ILLMClient
{
    IEnumerator SendChatRequest(
        string systemPrompt,//modele kim olduðunu ve kurallarý verir
        string userMessage,//kullanýcýnýn mesajý
        Action<string> onSuccess,
        Action<string> onError
    );
}