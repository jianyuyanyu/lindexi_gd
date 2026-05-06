using System.Text.Json.Serialization;

namespace VolcEngineSdk.OpenSpeech.Contexts;

/// <summary>
/// 混音参数。
/// </summary>
public sealed class SpeechSynthesisMixSpeaker
{
    /// <summary>
    /// 参与混音的音色列表。
    /// </summary>
    [JsonPropertyName("speakers")]
    public List<SpeechSynthesisMixSpeakerItem> Speakers { get; set; } = [];
}

/// <summary>
/// 混音音色项。
/// </summary>
public sealed class SpeechSynthesisMixSpeakerItem
{
    /// <summary>
    /// 源音色标识。
    /// </summary>
    [JsonPropertyName("source_speaker")]
    public string? SourceSpeaker { get; set; }

    /// <summary>
    /// 该音色在混音中的权重。
    /// </summary>
    [JsonPropertyName("mix_factor")]
    public double? MixFactor { get; set; }
}
