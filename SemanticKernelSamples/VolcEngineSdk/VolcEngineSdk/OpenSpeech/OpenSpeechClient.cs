using System.Buffers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using VolcEngineSdk.OpenSpeech.Contexts;

namespace VolcEngineSdk.OpenSpeech;

public sealed class OpenSpeechClient
{
    private const string DefaultBaseUrl = "https://openspeech.bytedance.com/api/v3/tts";
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    private readonly HttpClient _httpClient;

    public OpenSpeechClient(HttpClient? httpClient = null, string baseUrl = DefaultBaseUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(baseUrl);
        _httpClient = httpClient ?? new HttpClient();
        BaseUrl = baseUrl.TrimEnd('/');
    }

    public string BaseUrl { get; }

    /// <summary>
    /// 调用火山引擎 OpenSpeech 语音合成接口并返回完整音频数据。
    /// </summary>
    public async Task<SpeechSynthesisResult> SynthesizeAsync(SpeechSynthesisRequest request, SpeechSynthesisRequestOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(options);

        ValidateRequest(request);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, BuildRequestUri(options.Protocol));
        options.Apply(httpRequest.Headers);
        httpRequest.Content = JsonContent.Create(request, options: SerializerOptions);

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var logId = response.Headers.TryGetValues("X-Tt-Logid", out var values)
            ? values.FirstOrDefault()
            : null;

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        return options.Protocol switch
        {
            SpeechSynthesisProtocol.HttpChunked => await ReadChunkedResponseAsync(stream, logId, cancellationToken).ConfigureAwait(false),
            SpeechSynthesisProtocol.ServerSentEvents => await ReadServerSentEventsResponseAsync(stream, logId, cancellationToken).ConfigureAwait(false),
            _ => throw new ArgumentOutOfRangeException(nameof(options))
        };
    }

    private Uri BuildRequestUri(SpeechSynthesisProtocol protocol)
    {
        var path = protocol switch
        {
            SpeechSynthesisProtocol.HttpChunked => "unidirectional",
            SpeechSynthesisProtocol.ServerSentEvents => "unidirectional/sse",
            _ => throw new ArgumentOutOfRangeException(nameof(protocol))
        };

        return new Uri($"{BaseUrl}/{path}", UriKind.Absolute);
    }

    private static void ValidateRequest(SpeechSynthesisRequest request)
    {
        var requestParameters = request.RequestParameters ?? throw new ArgumentException(null, nameof(request));
        _ = requestParameters.AudioParameters ?? throw new ArgumentException(null, nameof(request));

        if (string.IsNullOrWhiteSpace(requestParameters.Text) && string.IsNullOrWhiteSpace(requestParameters.Ssml))
        {
            throw new ArgumentException(null, nameof(request));
        }

        if (string.IsNullOrWhiteSpace(requestParameters.Speaker))
        {
            throw new ArgumentException(null, nameof(request));
        }
    }

    private static async Task<SpeechSynthesisResult> ReadChunkedResponseAsync(Stream stream, string? logId, CancellationToken cancellationToken)
    {
        var state = new SpeechSynthesisAccumulator(logId);

        await foreach (var jsonDocument in JsonStreamDocumentReader.ReadAsync(stream, cancellationToken).ConfigureAwait(false))
        {
            using (jsonDocument)
            {
                var payload = jsonDocument.Deserialize<SpeechSynthesisResponse>(SerializerOptions);
                state.Append(payload);
            }
        }

        return state.Build();
    }

    private static async Task<SpeechSynthesisResult> ReadServerSentEventsResponseAsync(Stream stream, string? logId, CancellationToken cancellationToken)
    {
        var state = new SpeechSynthesisAccumulator(logId);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var dataBuilder = new StringBuilder();

        while (await reader.ReadLineAsync(cancellationToken).ConfigureAwait(false) is { } line)
        {
            if (line.Length == 0)
            {
                AppendServerSentEvent(dataBuilder, state);
                continue;
            }

            if (!line.StartsWith("data:", StringComparison.Ordinal))
            {
                continue;
            }

            if (dataBuilder.Length > 0)
            {
                dataBuilder.Append('\n');
            }

            dataBuilder.Append(line.AsSpan(5).TrimStart());
        }

        AppendServerSentEvent(dataBuilder, state);
        return state.Build();
    }

    private static void AppendServerSentEvent(StringBuilder dataBuilder, SpeechSynthesisAccumulator state)
    {
        if (dataBuilder.Length == 0)
        {
            return;
        }

        var payload = JsonSerializer.Deserialize<SpeechSynthesisResponse>(dataBuilder.ToString(), SerializerOptions);
        state.Append(payload);
        dataBuilder.Clear();
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        serializerOptions.Converters.Add(new SpeechSynthesisAdditionsJsonConverter());
        return serializerOptions;
    }

    private sealed class SpeechSynthesisAccumulator(string? logId)
    {
        private readonly MemoryStream _audioStream = new();
        private readonly List<SpeechSynthesisSentence> _sentences = [];
        private SpeechSynthesisUsage? _usage;

        public void Append(SpeechSynthesisResponse? response)
        {
            if (response is null)
            {
                return;
            }

            if (response.Code is 0)
            {
                if (!string.IsNullOrWhiteSpace(response.Data))
                {
                    var audioBytes = Convert.FromBase64String(response.Data);
                    _audioStream.Write(audioBytes);
                }

                if (response.Sentence is not null)
                {
                    _sentences.Add(response.Sentence);
                }

                return;
            }

            if (response.Code is 20000000)
            {
                _usage = response.Usage;
                return;
            }

            throw new InvalidOperationException(response.Message);
        }

        public SpeechSynthesisResult Build()
        {
            return new SpeechSynthesisResult
            {
                AudioData = _audioStream.ToArray(),
                Sentences = _sentences,
                Usage = _usage,
                LogId = logId
            };
        }
    }

    private static class JsonStreamDocumentReader
    {
        public static async IAsyncEnumerable<JsonDocument> ReadAsync(Stream stream, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(4096);
            byte[] pendingBuffer = Array.Empty<byte>();
            var pendingCount = 0;
            var readerOptions = new JsonReaderOptions { AllowMultipleValues = true };

            try
            {
                while (true)
                {
                    var bytesRead = await stream.ReadAsync(rentedBuffer, cancellationToken).ConfigureAwait(false);
                    if (bytesRead == 0)
                    {
                        break;
                    }

                    var combinedBuffer = new byte[pendingCount + bytesRead];
                    if (pendingCount > 0)
                    {
                        Buffer.BlockCopy(pendingBuffer, 0, combinedBuffer, 0, pendingCount);
                    }

                    Buffer.BlockCopy(rentedBuffer, 0, combinedBuffer, pendingCount, bytesRead);
                    pendingBuffer = ReadDocuments(combinedBuffer, readerOptions, isFinalBlock: false, out pendingCount, out var documents);

                    foreach (var document in documents)
                    {
                        yield return document;
                    }
                }

                if (pendingCount > 0)
                {
                    ReadDocuments(pendingBuffer[..pendingCount], readerOptions, isFinalBlock: true, out pendingCount, out var finalDocuments);
                    foreach (var document in finalDocuments)
                    {
                        yield return document;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedBuffer);
            }
        }

        private static byte[] ReadDocuments(byte[] buffer, JsonReaderOptions readerOptions, bool isFinalBlock, out int pendingCount, out List<JsonDocument> documents)
        {
            documents = [];
            var reader = new Utf8JsonReader(buffer, isFinalBlock, new JsonReaderState(readerOptions));

            while (JsonDocument.TryParseValue(ref reader, out var document))
            {
                documents.Add(document);
            }

            var consumed = (int) reader.BytesConsumed;
            pendingCount = buffer.Length - consumed;
            if (pendingCount == 0)
            {
                return Array.Empty<byte>();
            }

            var pendingBuffer = new byte[pendingCount];
            Buffer.BlockCopy(buffer, consumed, pendingBuffer, 0, pendingCount);
            return pendingBuffer;
        }
    }
}
