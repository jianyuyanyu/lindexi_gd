using VolcEngineSdk.OpenSpeech.Contexts;

namespace VolcEngineSdk.OpenSpeech;

public sealed class SpeechSynthesisResult
{
    public byte[] AudioData { get; init; } = [];

    public IReadOnlyList<SpeechSynthesisSentence> Sentences { get; init; } = [];

    public SpeechSynthesisUsage? Usage { get; init; }

    public string? LogId { get; init; }
}
