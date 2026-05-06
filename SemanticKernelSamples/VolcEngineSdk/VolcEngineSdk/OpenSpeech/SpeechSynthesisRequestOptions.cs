using System.Net.Http.Headers;

namespace VolcEngineSdk.OpenSpeech;

public sealed class SpeechSynthesisRequestOptions
{
    public SpeechSynthesisRequestOptions(OpenSpeechAuthentication authentication)
    {
        ArgumentNullException.ThrowIfNull(authentication);
        Authentication = authentication;
    }

    public OpenSpeechAuthentication Authentication { get; }

    public SpeechSynthesisProtocol Protocol { get; init; } = SpeechSynthesisProtocol.HttpChunked;

    public string? RequestId { get; init; }

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
