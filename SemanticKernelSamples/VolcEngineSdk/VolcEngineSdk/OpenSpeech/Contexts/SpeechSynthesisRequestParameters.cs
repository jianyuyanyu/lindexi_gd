using System.Text.Json.Serialization;

namespace VolcEngineSdk.OpenSpeech.Contexts;

/// <summary>
/// 语音合成请求参数。
/// </summary>
public sealed class SpeechSynthesisRequestParameters
{
    /// <summary>
    /// 输入文本；与 <see cref="Ssml"/> 至少填写一个。
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    /// <summary>
    /// 模型版本，例如 <c>seed-tts-1.1</c>。
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    /// <summary>
    /// SSML 文本；与 <see cref="Text"/> 至少填写一个。
    /// </summary>
    [JsonPropertyName("ssml")]
    public string? Ssml { get; set; }

    /// <summary>
    /// 发音人标识。
    /// </summary>
    [JsonPropertyName("speaker")]
    public string? Speaker { get; set; }

    /// <summary>
    /// 音频参数。
    /// </summary>
    [JsonPropertyName("audio_params")]
    public SpeechSynthesisAudioParameters? AudioParameters { get; set; }

    /// <summary>
    /// 追加的高级参数。
    /// </summary>
    [JsonPropertyName("additions")]
    public SpeechSynthesisAdditions? Additions { get; set; }

    /// <summary>
    /// 混音参数，仅在支持混音的场景下使用。
    /// </summary>
    [JsonPropertyName("mix_speaker")]
    public SpeechSynthesisMixSpeaker? MixSpeaker { get; set; }
}
