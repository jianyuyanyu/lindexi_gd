using System.Text.Json;
using System.Text.Json.Serialization;
using VolcEngineSdk.OpenSpeech.Contexts;

namespace VolcEngineSdk.OpenSpeech;

internal sealed class SpeechSynthesisAdditionsJsonConverter : JsonConverter<SpeechSynthesisAdditions>
{
    public override SpeechSynthesisAdditions? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var json = reader.GetString();
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonSerializer.Deserialize(json, OpenSpeechJsonSerializerContext.Default.SpeechSynthesisAdditions);
        }

        return JsonSerializer.Deserialize(ref reader, OpenSpeechJsonSerializerContext.Default.SpeechSynthesisAdditions);
    }

    public override void Write(Utf8JsonWriter writer, SpeechSynthesisAdditions value, JsonSerializerOptions options)
    {
        var json = JsonSerializer.Serialize(value, OpenSpeechJsonSerializerContext.Default.SpeechSynthesisAdditions);
        writer.WriteStringValue(json);
    }
}
