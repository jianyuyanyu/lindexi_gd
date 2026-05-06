using System.Text.Json.Serialization;

namespace VolcEngineSdk.OpenSpeech.Contexts;

public sealed class SpeechSynthesisRequestParameters
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("ssml")]
    public string? Ssml { get; set; }

    [JsonPropertyName("speaker")]
    public string? Speaker { get; set; }

    [JsonPropertyName("audio_params")]
    public SpeechSynthesisAudioParameters? AudioParameters { get; set; }

    [JsonPropertyName("additions")]
    public SpeechSynthesisAdditions? Additions { get; set; }

    [JsonPropertyName("mix_speaker")]
    public SpeechSynthesisMixSpeaker? MixSpeaker { get; set; }
}
