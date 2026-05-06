using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace VolcEngineSdk.OpenSpeech.Contexts;

public sealed class SpeechSynthesisAdditions
{
    [JsonPropertyName("silence_duration")]
    public int? SilenceDuration { get; set; }

    [JsonPropertyName("enable_language_detector")]
    public bool? EnableLanguageDetector { get; set; }

    [JsonPropertyName("disable_markdown_filter")]
    public bool? DisableMarkdownFilter { get; set; }

    [JsonPropertyName("disable_emoji_filter")]
    public bool? DisableEmojiFilter { get; set; }

    [JsonPropertyName("mute_cut_remain_ms")]
    public string? MuteCutRemainMs { get; set; }

    [JsonPropertyName("mute_cut_threshold")]
    public string? MuteCutThreshold { get; set; }

    [JsonPropertyName("enable_latex_tn")]
    public bool? EnableLatexTextNormalization { get; set; }

    [JsonPropertyName("latex_parser")]
    public string? LatexParser { get; set; }

    [JsonPropertyName("max_length_to_filter_parenthesis")]
    public int? MaxLengthToFilterParenthesis { get; set; }

    [JsonPropertyName("explicit_language")]
    public string? ExplicitLanguage { get; set; }

    [JsonPropertyName("context_language")]
    public string? ContextLanguage { get; set; }

    [JsonPropertyName("explicit_dialect")]
    public string? ExplicitDialect { get; set; }

    [JsonPropertyName("unsupported_char_ratio_thresh")]
    public double? UnsupportedCharRatioThreshold { get; set; }

    [JsonPropertyName("aigc_watermark")]
    public bool? AigcWatermark { get; set; }

    [JsonPropertyName("aigc_metadata")]
    public SpeechSynthesisAigcMetadata? AigcMetadata { get; set; }

    [JsonPropertyName("cache_config")]
    public SpeechSynthesisCacheConfig? CacheConfig { get; set; }

    [JsonPropertyName("post_process")]
    public SpeechSynthesisPostProcess? PostProcess { get; set; }

    [JsonPropertyName("context_texts")]
    public List<string>? ContextTexts { get; set; }

    [JsonPropertyName("use_tag_parser")]
    public bool? UseTagParser { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonNode?> ExtensionData { get; set; } = [];
}

public sealed class SpeechSynthesisAigcMetadata
{
    [JsonPropertyName("enable")]
    public bool? Enable { get; set; }

    [JsonPropertyName("content_producer")]
    public string? ContentProducer { get; set; }

    [JsonPropertyName("produce_id")]
    public string? ProduceId { get; set; }

    [JsonPropertyName("content_propagator")]
    public string? ContentPropagator { get; set; }

    [JsonPropertyName("propagate_id")]
    public string? PropagateId { get; set; }
}

public sealed class SpeechSynthesisCacheConfig
{
    [JsonPropertyName("text_type")]
    public int? TextType { get; set; }

    [JsonPropertyName("use_cache")]
    public bool? UseCache { get; set; }
}

public sealed class SpeechSynthesisPostProcess
{
    [JsonPropertyName("pitch")]
    public int? Pitch { get; set; }
}
