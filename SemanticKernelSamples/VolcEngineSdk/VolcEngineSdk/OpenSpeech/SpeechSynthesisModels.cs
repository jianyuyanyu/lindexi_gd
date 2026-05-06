using System.Text.Json.Serialization;

namespace VolcEngineSdk.OpenSpeech;

public sealed class SpeechSynthesisSentence
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("words")]
    public List<SpeechSynthesisWord> Words { get; set; } = [];
}

public sealed class SpeechSynthesisWord
{
    [JsonPropertyName("confidence")]
    public double? Confidence { get; set; }

    [JsonPropertyName("startTime")]
    public double? StartTime { get; set; }

    [JsonPropertyName("endTime")]
    public double? EndTime { get; set; }

    [JsonPropertyName("word")]
    public string? Word { get; set; }
}

public sealed class SpeechSynthesisUsage
{
    [JsonPropertyName("text_words")]
    public int? TextWords { get; set; }
}
