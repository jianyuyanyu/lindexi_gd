using System.Text.Json.Serialization;

namespace VolcEngineSdk.OpenSpeech.Contexts;

public sealed class SpeechSynthesisAudioParameters
{
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }

    [JsonPropertyName("bit_rate")]
    public int? BitRate { get; set; }

    [JsonPropertyName("emotion")]
    public string? Emotion { get; set; }

    [JsonPropertyName("emotion_scale")]
    public int? EmotionScale { get; set; }

    [JsonPropertyName("speech_rate")]
    public int? SpeechRate { get; set; }

    [JsonPropertyName("loudness_rate")]
    public int? LoudnessRate { get; set; }

    [JsonPropertyName("enable_timestamp")]
    public bool? EnableTimestamp { get; set; }

    [JsonPropertyName("enable_subtitle")]
    public bool? EnableSubtitle { get; set; }
}
