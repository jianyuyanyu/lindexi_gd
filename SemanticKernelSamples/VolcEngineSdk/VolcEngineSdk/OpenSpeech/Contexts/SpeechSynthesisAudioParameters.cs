using System.Text.Json.Serialization;

namespace VolcEngineSdk.OpenSpeech.Contexts;

/// <summary>
/// 音频输出参数。
/// </summary>
public sealed class SpeechSynthesisAudioParameters
{
    /// <summary>
    /// 音频编码格式，例如 <c>mp3</c>、<c>ogg_opus</c>、<c>pcm</c>。
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; set; }

    /// <summary>
    /// 音频采样率。
    /// </summary>
    [JsonPropertyName("sample_rate")]
    public int? SampleRate { get; set; }

    /// <summary>
    /// 音频比特率，主要对 MP3 格式生效。
    /// </summary>
    [JsonPropertyName("bit_rate")]
    public int? BitRate { get; set; }

    /// <summary>
    /// 音色情感，例如 <c>angry</c>。
    /// </summary>
    [JsonPropertyName("emotion")]
    public string? Emotion { get; set; }

    /// <summary>
    /// 情感强度，范围通常为 1 到 5。
    /// </summary>
    [JsonPropertyName("emotion_scale")]
    public int? EmotionScale { get; set; }

    /// <summary>
    /// 语速，取值范围通常为 -50 到 100。
    /// </summary>
    [JsonPropertyName("speech_rate")]
    public int? SpeechRate { get; set; }

    /// <summary>
    /// 音量，取值范围通常为 -50 到 100。
    /// </summary>
    [JsonPropertyName("loudness_rate")]
    public int? LoudnessRate { get; set; }

    /// <summary>
    /// 是否返回句级时间戳信息。
    /// </summary>
    [JsonPropertyName("enable_timestamp")]
    public bool? EnableTimestamp { get; set; }

    /// <summary>
    /// 是否返回字幕信息。
    /// </summary>
    [JsonPropertyName("enable_subtitle")]
    public bool? EnableSubtitle { get; set; }
}
