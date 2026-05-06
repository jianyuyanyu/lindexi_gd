using VolcEngineSdk.OpenSpeech.Contexts;

namespace VolcEngineSdk.OpenSpeech;

/// <summary>
/// 语音合成完成后的结果。
/// </summary>
public sealed class SpeechSynthesisResult
{
    /// <summary>
    /// 合成得到的完整音频数据。
    /// </summary>
    public byte[] AudioData { get; init; } = [];

    /// <summary>
    /// 合成过程中的分句结果。
    /// </summary>
    public IReadOnlyList<SpeechSynthesisSentence> Sentences { get; init; } = [];

    /// <summary>
    /// 服务端返回的用量统计信息。
    /// </summary>
    public SpeechSynthesisUsage? Usage { get; init; }

    /// <summary>
    /// 服务端返回的日志 ID，便于排查问题。
    /// </summary>
    public string? LogId { get; init; }
}
