namespace VolcEngineSdk.OpenSpeech;

/// <summary>
/// 语音合成请求支持的传输协议。
/// </summary>
public enum SpeechSynthesisProtocol
{
    /// <summary>
    /// HTTP Chunked 流式响应。
    /// </summary>
    HttpChunked,

    /// <summary>
    /// HTTP Server-Sent Events 流式响应。
    /// </summary>
    ServerSentEvents
}
