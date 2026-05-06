using System.Net.Http.Headers;

namespace VolcEngineSdk.OpenSpeech;

/// <summary>
/// 语音合成请求的通用选项。
/// </summary>
public sealed class SpeechSynthesisRequestOptions
{
    /// <summary>
    /// 初始化一个 <see cref="SpeechSynthesisRequestOptions"/> 实例。
    /// </summary>
    /// <param name="authentication">请求鉴权信息。</param>
    public SpeechSynthesisRequestOptions(OpenSpeechAuthentication authentication)
    {
        ArgumentNullException.ThrowIfNull(authentication);
        Authentication = authentication;
    }

    /// <summary>
    /// 请求鉴权信息。
    /// </summary>
    public OpenSpeechAuthentication Authentication { get; }

    /// <summary>
    /// 请求使用的传输协议，默认使用 HTTP Chunked。
    /// </summary>
    public SpeechSynthesisProtocol Protocol { get; init; } = SpeechSynthesisProtocol.HttpChunked;

    /// <summary>
    /// 可选的客户端请求 ID，会写入 <c>X-Api-Request-Id</c> 请求头。
    /// </summary>
    public string? RequestId { get; init; }

    /// <summary>
    /// 可选的用量返回标记，会写入 <c>X-Control-Require-Usage-Tokens-Return</c> 请求头。
    /// </summary>
    public string? UsageTokensToReturn { get; init; }

    internal void Apply(HttpRequestHeaders headers)
    {
        ArgumentNullException.ThrowIfNull(headers);

        Authentication.Apply(headers);

        if (!string.IsNullOrWhiteSpace(RequestId))
        {
            headers.Add("X-Api-Request-Id", RequestId);
        }

        if (!string.IsNullOrWhiteSpace(UsageTokensToReturn))
        {
            headers.Add("X-Control-Require-Usage-Tokens-Return", UsageTokensToReturn);
        }
    }
}
