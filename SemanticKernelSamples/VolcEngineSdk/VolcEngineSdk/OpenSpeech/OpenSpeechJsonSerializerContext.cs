using System.Text.Json;
using System.Text.Json.Serialization;
using VolcEngineSdk.OpenSpeech.Contexts;

namespace VolcEngineSdk.OpenSpeech;

[JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(SpeechSynthesisRequest))]
[JsonSerializable(typeof(SpeechSynthesisRequestParameters))]
[JsonSerializable(typeof(SpeechSynthesisAudioParameters))]
[JsonSerializable(typeof(SpeechSynthesisAdditions))]
[JsonSerializable(typeof(SpeechSynthesisAigcMetadata))]
[JsonSerializable(typeof(SpeechSynthesisCacheConfig))]
[JsonSerializable(typeof(SpeechSynthesisPostProcess))]
[JsonSerializable(typeof(SpeechSynthesisMixSpeaker))]
[JsonSerializable(typeof(SpeechSynthesisMixSpeakerItem))]
[JsonSerializable(typeof(UserMeta))]
[JsonSerializable(typeof(SpeechSynthesisResponse))]
[JsonSerializable(typeof(SpeechSynthesisSentence))]
[JsonSerializable(typeof(SpeechSynthesisWord))]
[JsonSerializable(typeof(SpeechSynthesisUsage))]
internal sealed partial class OpenSpeechJsonSerializerContext : JsonSerializerContext
{
}
