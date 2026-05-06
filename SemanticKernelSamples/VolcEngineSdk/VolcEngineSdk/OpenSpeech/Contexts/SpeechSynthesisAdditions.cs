using System.Text.Json;
using System.Text.Json.Serialization;

namespace VolcEngineSdk.OpenSpeech.Contexts;

/// <summary>
/// 语音合成的扩展参数。
/// </summary>
public sealed class SpeechSynthesisAdditions
{
    /// <summary>
    /// 句尾静音时长，单位毫秒。
    /// </summary>
    [JsonPropertyName("silence_duration")]
    public int? SilenceDuration { get; set; }

    /// <summary>
    /// 是否启用语种自动识别。
    /// </summary>
    [JsonPropertyName("enable_language_detector")]
    public bool? EnableLanguageDetector { get; set; }

    /// <summary>
    /// 是否过滤 Markdown 语法。
    /// </summary>
    [JsonPropertyName("disable_markdown_filter")]
    public bool? DisableMarkdownFilter { get; set; }

    /// <summary>
    /// 是否保留文本中的 Emoji。
    /// </summary>
    [JsonPropertyName("disable_emoji_filter")]
    public bool? DisableEmojiFilter { get; set; }

    /// <summary>
    /// 静音截断保留时长，字符串形式传递。
    /// </summary>
    [JsonPropertyName("mute_cut_remain_ms")]
    public string? MuteCutRemainMs { get; set; }

    /// <summary>
    /// 静音判断阈值，字符串形式传递。
    /// </summary>
    [JsonPropertyName("mute_cut_threshold")]
    public string? MuteCutThreshold { get; set; }

    /// <summary>
    /// 是否启用 LaTeX 文本归一化播报。
    /// </summary>
    [JsonPropertyName("enable_latex_tn")]
    public bool? EnableLatexTextNormalization { get; set; }

    /// <summary>
    /// LaTeX 解析器配置。
    /// </summary>
    [JsonPropertyName("latex_parser")]
    public string? LatexParser { get; set; }

    /// <summary>
    /// 括号内文本过滤长度阈值。
    /// </summary>
    [JsonPropertyName("max_length_to_filter_parenthesis")]
    public int? MaxLengthToFilterParenthesis { get; set; }

    /// <summary>
    /// 明确语种。
    /// </summary>
    [JsonPropertyName("explicit_language")]
    public string? ExplicitLanguage { get; set; }

    /// <summary>
    /// 参考语种。
    /// </summary>
    [JsonPropertyName("context_language")]
    public string? ContextLanguage { get; set; }

    /// <summary>
    /// 明确方言。
    /// </summary>
    [JsonPropertyName("explicit_dialect")]
    public string? ExplicitDialect { get; set; }

    /// <summary>
    /// 不支持字符比例阈值。
    /// </summary>
    [JsonPropertyName("unsupported_char_ratio_thresh")]
    public double? UnsupportedCharRatioThreshold { get; set; }

    /// <summary>
    /// 是否开启 AIGC 水印。
    /// </summary>
    [JsonPropertyName("aigc_watermark")]
    public bool? AigcWatermark { get; set; }

    /// <summary>
    /// AIGC 元数据配置。
    /// </summary>
    [JsonPropertyName("aigc_metadata")]
    public SpeechSynthesisAigcMetadata? AigcMetadata { get; set; }

    /// <summary>
    /// 缓存配置。
    /// </summary>
    [JsonPropertyName("cache_config")]
    public SpeechSynthesisCacheConfig? CacheConfig { get; set; }

    /// <summary>
    /// 后处理配置。
    /// </summary>
    [JsonPropertyName("post_process")]
    public SpeechSynthesisPostProcess? PostProcess { get; set; }

    /// <summary>
    /// 对话式上下文文本，仅在支持的模型上生效。
    /// </summary>
    [JsonPropertyName("context_texts")]
    public List<string>? ContextTexts { get; set; }

    /// <summary>
    /// 是否启用 Cot 标签解析。
    /// </summary>
    [JsonPropertyName("use_tag_parser")]
    public bool? UseTagParser { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtensionData { get; set; } = [];
}

public sealed class SpeechSynthesisAigcMetadata
{
    /// <summary>
    /// 是否启用隐式水印。
    /// </summary>
    [JsonPropertyName("enable")]
    public bool? Enable { get; set; }

    /// <summary>
    /// 合成服务提供者名称或编码。
    /// </summary>
    [JsonPropertyName("content_producer")]
    public string? ContentProducer { get; set; }

    /// <summary>
    /// 内容制作编号。
    /// </summary>
    [JsonPropertyName("produce_id")]
    public string? ProduceId { get; set; }

    /// <summary>
    /// 内容传播服务提供者名称或编码。
    /// </summary>
    [JsonPropertyName("content_propagator")]
    public string? ContentPropagator { get; set; }

    /// <summary>
    /// 内容传播编号。
    /// </summary>
    [JsonPropertyName("propagate_id")]
    public string? PropagateId { get; set; }
}

public sealed class SpeechSynthesisCacheConfig
{
    /// <summary>
    /// 缓存文本类型。
    /// </summary>
    [JsonPropertyName("text_type")]
    public int? TextType { get; set; }

    /// <summary>
    /// 是否使用缓存。
    /// </summary>
    [JsonPropertyName("use_cache")]
    public bool? UseCache { get; set; }
}

public sealed class SpeechSynthesisPostProcess
{
    /// <summary>
    /// 音调调整值，范围通常为 -12 到 12。
    /// </summary>
    [JsonPropertyName("pitch")]
    public int? Pitch { get; set; }
}
