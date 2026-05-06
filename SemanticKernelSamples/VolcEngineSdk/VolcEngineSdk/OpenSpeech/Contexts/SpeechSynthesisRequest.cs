using System.Text.Json.Serialization;

namespace VolcEngineSdk.OpenSpeech.Contexts;

/// <summary>
/// 语音合成请求根对象。
/// </summary>
public sealed class SpeechSynthesisRequest
{
    /// <summary>
    /// 用户信息。
    /// </summary>
    [JsonPropertyName("user")]
    public UserMeta? User { get; set; }

    /// <summary>
    /// 语音合成请求参数。
    /// </summary>
    [JsonPropertyName("req_params")]
    public SpeechSynthesisRequestParameters? RequestParameters { get; set; }
}
