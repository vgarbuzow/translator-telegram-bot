using System.Text.Json.Serialization;

namespace ReceivedTranslated
{
    internal class TranslatedMessage
    {
        [JsonPropertyName("detectedLanguage")]
        public DetectedLanguage DetectedLanguage { get; set; }

        [JsonPropertyName("translations")]
        public Translations Translations { get; set; }
    }
    internal class DetectedLanguage
    {
        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("score")]
        public double Score { get; set; }
    }

    internal class Translations
    {
        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("to")]
        public string To { get; set; }
    }

}
