
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class LLMResponseParser
{
    public bool TryParse(
        string rawContent,
        out LLMResponse parsedResponse,
        out string errorMessage)
    {
        parsedResponse = null;
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(rawContent))
        {
            errorMessage = "Model boş cevap döndürdü.";
            return false;
        }

        try
        {
            JObject json;
            JsonLoadSettings settings = new JsonLoadSettings
            {
                //aynı alan adı olursa hata yolluyor
                DuplicatePropertyNameHandling =DuplicatePropertyNameHandling.Error
            };

            //ollamadan gelen cevabı JSON olarak ayrıştır
            using (StringReader stringReader = new StringReader(rawContent))
            using (JsonTextReader jsonReader = new JsonTextReader(stringReader))
            {
                jsonReader.DateParseHandling =DateParseHandling.None;//tarihleri otomatik olarak ayrıştırma

                JToken root = JToken.ReadFrom(jsonReader,settings);
            
                if (!(root is JObject))
                {
                    errorMessage ="LLM cevabı JSON nesnesi değil.";
                    return false;
                }

                // JSON nesnesinden sonra ek içerik olamaz
                if (jsonReader.Read())
                {
                    errorMessage ="JSON cevabında fazladan içerik var.";
                    return false;
                }

                json = (JObject)root;
            }

            // Yalnızca üç alana izin verir
            foreach (JProperty property in json.Properties())
            {
                if (property.Name != "intent" &&
                    property.Name != "dialogue" &&
                    property.Name != "action")
                {
                    errorMessage ="LLM cevabında beklenmeyen alan var: " +property.Name;

                    return false;
                }
            }

            // Zorunlu alanları ve veri tiplerini kontrol et
            if (!TryReadString(
                json,
                "intent",
                out string intent,
                out errorMessage))
            {
                return false;
            }

            if (!TryReadString(
                json,
                "dialogue",
                out string dialogue,
                out errorMessage))
            {
                return false;
            }

            if (!TryReadString(
                json,
                "action",
                out string action,
                out errorMessage))
            {
                return false;
            }

            // Geçerli niyet değerlerini kontrol et
            if (intent != "EvacuationRequest" &&intent != "ThreatOrSabotage" &&intent != "OtherOrUnclear")
            {
                errorMessage ="LLM geçersiz veya belirsiz intent üretti.";
                return false;
            }

            // Geçerli action değerlerini kontrol et
            if (action != "OPEN_EXIT" &&action != "REQUEST_MORE_INFO" &&action != "DENY")
            {
                errorMessage ="LLM geçersiz action üretti.";
                return false;
            }

            // Teknik action isimleri doğal konuşmanın içinde bulunmamalı.
            if (dialogue.IndexOf(
                    "OPEN_EXIT",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                dialogue.IndexOf(
                    "REQUEST_MORE_INFO",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                dialogue.IndexOf(
                    "DENY",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                errorMessage =
                    "LLM konuşmasında teknik action ismi bulundu.";
                return false;
            }

            // Doğrulanmış yanıt nesnesini oluştur
            parsedResponse = new LLMResponse
            {
                intent = intent,
                dialogue = dialogue,
                action = action
            };

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning(
                "LLM JSON ayrıştırma hatası: " + exception.GetType().Name
            );

            errorMessage ="Model geçersiz JSON döndürdü.";
            parsedResponse = null;
            return false;
        }
    }

    // Zorunlu JSON alanının string olduğunu kontrol eder.
    private bool TryReadString(
        JObject json,
        string fieldName,
        out string value,
        out string errorMessage)
    {
        value = null;
        errorMessage = null;

        JToken token = json[fieldName];

        if (token == null)
        {
            errorMessage ="LLM cevabında " + fieldName +" alanı eksik.";

            return false;
        }

        if (token.Type != JTokenType.String)
        {
            errorMessage ="LLM cevabında " + fieldName +" alanının tipi string değil.";

            return false;
        }

        value = token.Value<string>()?.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            errorMessage ="LLM cevabında " + fieldName +" alanı boş.";

            return false;
        }

        return true;
    }
}
