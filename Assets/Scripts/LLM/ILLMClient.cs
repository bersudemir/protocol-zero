
using System;
using System.Collections;

public interface ILLMClient
{
    IEnumerator SendChatRequest(
        string systemPrompt,
        string userMessage,
        bool canOpenExit,
        Action<string> onSuccess,
        Action<string> onError
    );
}
