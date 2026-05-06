using System.Text.Json.Serialization;

namespace VolcEngineSdk.OpenSpeech.Contexts;

public sealed class SpeechSynthesisMixSpeaker
{
    [JsonPropertyName("speakers")]
    public List<SpeechSynthesisMixSpeakerItem> Speakers { get; set; } = [];
}

public sealed class SpeechSynthesisMixSpeakerItem
{
    [JsonPropertyName("source_speaker")]
    public string? SourceSpeaker { get; set; }

    [JsonPropertyName("mix_factor")]
    public double? MixFactor { get; set; }
}
