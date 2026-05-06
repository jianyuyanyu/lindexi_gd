using VolcEngineSdk.OpenSpeech;
using VolcEngineSdk.OpenSpeech.Contexts;

var accessTokenFile = @"C:\lindexi\Work\Key\OpenSpeech TTS Access Token.txt";
var appId = "5866932789";

// speaker 发音人： voiceType
var voiceType = "zh_female_vv_uranus_bigtts";

// 将 use_tag_parser 开启。开启cot解析能力。cot能力可以辅助当前语音合成，对语速、情感等进行调整
// 传递内容为：
// <cot text=急促难耐>工作占据了生活的绝大部分</cot>，只有去做自己认为伟大的工作，才能获得满足感。<cot text=语速缓慢>不管生活再苦再累，都绝不放弃寻找</cot>

// 模型版本： seed-tts-2.0-expressive
const string resourceId = "seed-tts-2.0";
const string model = "seed-tts-2.0-expressive";
const string outputFileName = "cot-demo.mp3";
const string text = "<cot text=急促难耐>工作占据了生活的绝大部分</cot>，只有去做自己认为伟大的工作，才能获得满足感。<cot text=语速缓慢>不管生活再苦再累，都绝不放弃寻找</cot>";

Console.WriteLine("开始调用 OpenSpeech 语音合成...");

var outputFilePath = Path.Combine(AppContext.BaseDirectory, outputFileName);
var authentication = CreateAuthentication(appId, accessTokenFile, resourceId);
var request = CreateRequest(voiceType, model, text);
var options = new SpeechSynthesisRequestOptions(authentication)
{
    Protocol = SpeechSynthesisProtocol.HttpChunked,
    RequestId = Guid.NewGuid().ToString(),
    UsageTokensToReturn = "text_words"
};

using var httpClient = new HttpClient();
var client = new OpenSpeechClient(httpClient);
var result = await client.SynthesizeAsync(request, options);

await File.WriteAllBytesAsync(outputFilePath, result.AudioData);

Console.WriteLine($"音频文件已生成: {outputFilePath}");
Console.WriteLine($"音频字节数: {result.AudioData.Length}");
Console.WriteLine($"返回句子数: {result.Sentences.Count}");
Console.WriteLine($"计费字符数: {result.Usage?.TextWords?.ToString() ?? "未返回"}");
Console.WriteLine($"服务端 LogId: {result.LogId ?? "未返回"}");

static OpenSpeechAuthentication CreateAuthentication(string appId, string accessTokenFile, string resourceId)
{
    var apiKey = Environment.GetEnvironmentVariable("OPENSPEECH_API_KEY");
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        return OpenSpeechAuthentication.CreateWithApiKey(apiKey.Trim(), resourceId);
    }

    ArgumentException.ThrowIfNullOrWhiteSpace(appId);

    var accessKey = ReadRequiredText(accessTokenFile);
    return OpenSpeechAuthentication.CreateWithLegacyCredentials(appId, accessKey, resourceId);
}

static SpeechSynthesisRequest CreateRequest(string voiceType, string model, string text)
{
    return new SpeechSynthesisRequest
    {
        User = new UserMeta
        {
            Uid = Environment.UserName
        },
        RequestParameters = new SpeechSynthesisRequestParameters
        {
            Text = text,
            Model = model,
            Speaker = voiceType,
            AudioParameters = new SpeechSynthesisAudioParameters
            {
                Format = "mp3",
                SampleRate = 24000
            },
            Additions = new SpeechSynthesisAdditions
            {
                UseTagParser = true
            }
        }
    };
}

static string ReadRequiredText(string filePath)
{
    if (!File.Exists(filePath))
    {
        throw new FileNotFoundException($"找不到密钥文件：{filePath}", filePath);
    }

    var text = File.ReadAllText(filePath).Trim();
    if (string.IsNullOrWhiteSpace(text))
    {
        throw new InvalidOperationException($"密钥文件内容为空：{filePath}");
    }

    return text;
}
