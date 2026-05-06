using System.Text.Json.Serialization;

namespace VolcEngineSdk.OpenSpeech.Contexts;

public sealed class SpeechSynthesisRequest
{
    [JsonPropertyName("user")]
    public UserMeta? User { get; set; }

    [JsonPropertyName("req_params")]
    public SpeechSynthesisRequestParameters? RequestParameters { get; set; }
}
