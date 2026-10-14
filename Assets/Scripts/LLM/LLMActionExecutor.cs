
using UnityEngine;

// Oyuncunun mesajýndan LLM tarafýndan belirlenen niyet.
// Bu bilgi gerçek Unity GameState yerine geçmez.
public enum PlayerIntent
{
    Unknown,
    EvacuationRequest,
    ThreatOrSabotage,
    OtherOrUnclear
}

public class LLMActionExecutor
{
    // Modelin önerdiði action'ý doðrular.
    // Modelin kararýný deðiþtirmez.
    // Gerçek oyun durumunu Unity'den okur.
    public bool CanApplyAction(
        string llmAction,
        PlayerIntent playerIntent,
        GameState gameState,
        out string validationFeedback)
    {
        validationFeedback = null;

        if (gameState == null)
        {
            validationFeedback = "GameState bulunamadý.";
            Debug.LogError(validationFeedback);
            return false;
        }

        if (string.IsNullOrWhiteSpace(llmAction))
        {
            validationFeedback = "LLM action boþ geldi.";
            Debug.LogWarning(validationFeedback);
            return false;
        }

        string action = llmAction.Trim();

        // Bilinmeyen niyetler güvenli þekilde reddedilir.
        if (playerIntent != PlayerIntent.EvacuationRequest &&
            playerIntent != PlayerIntent.ThreatOrSabotage &&
            playerIntent != PlayerIntent.OtherOrUnclear)
        {
            validationFeedback =
                "Oyuncunun niyeti güvenilir þekilde " +
                "belirlenemedi. Action uygulanmadý.";

            Debug.LogWarning(validationFeedback);
            return false;
        }

        // Gerçek Unity oyun koþullarý.
        bool exitRequirementsMet =
            gameState.HasKeycard &&
            gameState.PowerRestored;

        // 1. MODEL OPEN_EXIT ÖNERDÝYSE
        if (action == "OPEN_EXIT")
        {
            // Açýk tahliye niyeti bulunmalý.
            if (playerIntent != PlayerIntent.EvacuationRequest)
            {
                validationFeedback =
                    "OPEN_EXIT kararý belirtilen " +
                    "oyuncu niyetiyle çeliþiyor.";

                Debug.LogWarning(validationFeedback);
                return false;
            }

            // Fiziksel güvenlik þartlarý saðlanmalý.
            if (!exitRequirementsMet)
            {
                validationFeedback =
                    "OPEN_EXIT kararý gerçek Unity " +
                    "oyun durumuyla çeliþiyor. " +
                    $"HasKeycard: {gameState.HasKeycard}, " +
                    $"PowerRestored: {gameState.PowerRestored}.";

                Debug.LogWarning(validationFeedback);
                return false;
            }

            return true;
        }

        // 2. MODEL DENY ÖNERDÝYSE
        if (action == "DENY")
        {
            // Tehdit veya sabotaj reddedilir.
            if (playerIntent == PlayerIntent.ThreatOrSabotage)
            {
                return true;
            }

            // Tahliye talebi var ama oyun koþullarý eksik.
            if (playerIntent == PlayerIntent.EvacuationRequest &&
                !exitRequirementsMet)
            {
                return true;
            }

            validationFeedback =
                "DENY kararý belirtilen niyet ve " +
                "oyun durumuyla tutarsýz.";

            Debug.LogWarning(validationFeedback);
            return false;
        }

        // 3. MODEL REQUEST_MORE_INFO ÖNERDÝYSE
        if (action == "REQUEST_MORE_INFO")
        {
            // Belirsiz veya alakasýz isteklerde kabul edilir.
            if (playerIntent == PlayerIntent.OtherOrUnclear)
            {
                return true;
            }

            validationFeedback =
                "REQUEST_MORE_INFO kararý belirtilen " +
                "oyuncu niyetiyle tutarsýz.";

            Debug.LogWarning(validationFeedback);
            return false;
        }

        // 4. BÝLÝNMEYEN ACTION
        validationFeedback =
            "LLM bilinmeyen bir action üretti.";

        Debug.LogWarning(validationFeedback);
        return false;
    }

    // ESKÝ METOT:
    // Mevcut çaðrýlar bozulmasýn diye korunuyor.
    // Ancak niyet kontrolü yapamadýðý için
    // OPEN_EXIT yetkisi veremez.
    //
    // Kullanýldýðý bütün yerler incelendikten sonra
    // kaldýrýlmasý deðerlendirilecek.
    public bool CanApplyAction(
        string llmAction,
        GameState gameState,
        out string validationFeedback)
    {
        validationFeedback = null;

        if (gameState == null)
        {
            validationFeedback = "GameState bulunamadý.";
            Debug.LogError(validationFeedback);
            return false;
        }

        if (string.IsNullOrWhiteSpace(llmAction))
        {
            validationFeedback = "LLM action boþ geldi.";
            Debug.LogWarning(validationFeedback);
            return false;
        }

        string action = llmAction.Trim();

        // Niyet bilgisi olmadan kapý açma izni verilmez.
        if (action == "OPEN_EXIT")
        {
            validationFeedback =
                "Eski doðrulama metodu OPEN_EXIT " +
                "yetkisi veremez. Niyet doðrulamasý gerekli.";

            Debug.LogWarning(validationFeedback);
            return false;
        }

        // Kapý açmayan action'lar için eski uyumluluk.
        if (action == "DENY" ||
            action == "REQUEST_MORE_INFO")
        {
            return true;
        }

        validationFeedback =
            "Bilinmeyen LLM action.";

        Debug.LogWarning(validationFeedback);
        return false;
    }
}
