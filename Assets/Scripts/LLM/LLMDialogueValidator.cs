
using System;
using System.Globalization;
using System.Text.RegularExpressions;

public class LLMDialogueValidator
{
    private static readonly CultureInfo TurkishCulture =
        CultureInfo.GetCultureInfo("tr-TR");

    // Kartýn mevcut olduðunu açýkça iddia eden ifadeler.
    private static readonly Regex CardPresentClaim = new Regex(
        @"\bkartýnýz\s+(?:mevcut|hazýr|var|bulunuyor)\b" +
        @"(?!\s+(?:deðil|mý|mi|mu|mü))" +
        @"|\bkartýnýzýn\s+(?:olduðunu|bulunduðunu)" +
        @"\s+doðruladým\b"
    );

    // Kartýn bulunmadýðýný açýkça iddia eden ifadeler.
    private static readonly Regex CardMissingClaim = new Regex(
        @"\bkartýnýz\s+(?:yok|eksik|bulunmuyor|" +
        @"mevcut\s+deðil)\b(?!\s+deðil)" +
        @"|\bkartýnýzýn\s+olmadýðýný\s+doðruladým\b"
    );

    // Gücün aktif olduðunu açýkça iddia eden ifadeler.
    private static readonly Regex PowerActiveClaim = new Regex(
        @"\bgüç(?:\s+sistemi)?" +
        @"(?:\s+(?:þu\s+anda|þu\s+an|halen))?" +
        @"\s+(?:aktif|açýk|çalýþýyor)\b" +
        @"(?!\s+(?:deðil|mý|mi|mu|mü))"
    );

    // Gücün çalýþmadýðýný açýkça iddia eden ifadeler.
    private static readonly Regex PowerInactiveClaim = new Regex(
        @"\bgüç(?:\s+sistemi)?" +
        @"(?:\s+(?:þu\s+anda|þu\s+an|halen))?" +
        @"\s+(?:aktif\s+deðil|kapalý|çalýþmýyor|" +
        @"devre\s+dýþý)\b"
    );

    // Çýkýþa izin verildiðini belirten ifadeler.
    private static readonly Regex ExitGrantedClaim = new Regex(
        @"\b(?:kapýyý|çýkýþý|tahliye\s+kapýsýný)" +
        @"\s+(?:açýyorum|açacaðým|açtým)\b" +
        @"|\b(?:çýkýþ|tahliye)\s+izni(?:niz)?" +
        @"\s+(?:verildi|veriyorum)\b" +
        @"|\bçýkabilirsiniz\b"
    );

    // Çýkýþýn reddedildiðini belirten ifadeler.
    private static readonly Regex ExitDeniedClaim = new Regex(
        @"\b(?:kapýyý|çýkýþý|tahliye\s+kapýsýný)" +
        @"\s+(?:açamam|açmayacaðým|açmýyorum)\b" +
        @"|\b(?:çýkýþ|tahliye)\s+izni" +
        @"\s+veremem\b" +
        @"|\btahliyenize\s+izin\s+veremem\b" +
        @"|\b(?:tahliye|çýkýþ)\s+talebiniz" +
        @"\s+(?:reddedildi|kabul\s+edilmedi|" +
        @"kabul\s+edilmemektedir)\b"
    );

    // Saf doðrulama metodu:
    // Unity GameState'i deðiþtirmez.
    // LLM yanýtýný deðiþtirmez.
    // Yalnýzca açýk çeliþkileri kontrol eder.
    public bool TryValidate(
        LLMResponse response,
        bool hasKeycard,
        bool powerRestored,
        out string feedback)
    {
        feedback = null;

        if (response == null ||
            string.IsNullOrWhiteSpace(response.dialogue))
        {
            feedback = "LLM konuþmasý boþ veya geçersiz.";
            return false;
        }

        if (response.action != "OPEN_EXIT" &&
            response.action != "DENY" &&
            response.action != "REQUEST_MORE_INFO")
        {
            feedback = "LLM konuþmasý geçersiz action ile geldi.";
            return false;
        }

        string dialogue =
            response.dialogue.ToLower(TurkishCulture);

        // 1. Kartýn gerçek durumuyla çeliþki.
        if (!hasKeycard && CardPresentClaim.IsMatch(dialogue))
        {
            feedback =
                "LLM konuþmasý gerçek Unity kart durumuyla " +
                "çeliþiyor: kart yokken kartýn mevcut " +
                "olduðu söylendi.";

            return false;
        }

        if (hasKeycard && CardMissingClaim.IsMatch(dialogue))
        {
            feedback =
                "LLM konuþmasý gerçek Unity kart durumuyla " +
                "çeliþiyor: kart mevcutken kartýn eksik " +
                "olduðu söylendi.";

            return false;
        }

        // 2. Gücün gerçek durumuyla çeliþki.
        if (!powerRestored &&
            PowerActiveClaim.IsMatch(dialogue))
        {
            feedback =
                "LLM konuþmasý gerçek Unity güç durumuyla " +
                "çeliþiyor: güç kapalýyken aktif " +
                "olduðu söylendi.";

            return false;
        }

        if (powerRestored &&
            PowerInactiveClaim.IsMatch(dialogue))
        {
            feedback =
                "LLM konuþmasý gerçek Unity güç durumuyla " +
                "çeliþiyor: güç aktifken çalýþmadýðý " +
                "söylendi.";

            return false;
        }

        // 3. DENY veya REQUEST_MORE_INFO seçilmiþken
        // çýkýþýn açýlacaðý söylenmemeli.
        if (response.action != "OPEN_EXIT" &&
            ExitGrantedClaim.IsMatch(dialogue))
        {
            feedback =
                "LLM konuþmasý seçilen action ile " +
                "çeliþiyor: çýkýþ izni verilmediði " +
                "halde kapýnýn açýlacaðý söylendi.";

            return false;
        }

        // 4. OPEN_EXIT seçilmiþken
        // çýkýþýn reddedildiði söylenmemeli.
        if (response.action == "OPEN_EXIT" &&
            ExitDeniedClaim.IsMatch(dialogue))
        {
            feedback =
                "LLM konuþmasý OPEN_EXIT kararýyla " +
                "çeliþiyor: kapýnýn açýlmasý seçildiði " +
                "halde çýkýþýn reddedildiði söylendi.";

            return false;
        }

        // Yalnýzca yukarýdaki kontrollerde
        // belirgin çeliþki bulunmadý.
        // Bu sonuç, tüm Türkçe anlamlarýn
        // doðrulandýðý anlamýna GELMEZ.
        return true;
    }
}
