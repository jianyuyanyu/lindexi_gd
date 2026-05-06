using System.Text.Json.Serialization;

namespace VolcEngineSdk.OpenSpeech.Contexts;

internal sealed class SpeechSynthesisResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("sentence")]
    public SpeechSynthesisSentence? Sentence { get; set; }

    [JsonPropertyName("usage")]
    public SpeechSynthesisUsage? Usage { get; set; }
}
