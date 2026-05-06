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

            return JsonSerializer.Deserialize<SpeechSynthesisAdditions>(json, options);
        }

        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        return jsonDocument.Deserialize<SpeechSynthesisAdditions>(options);
    }

    public override void Write(Utf8JsonWriter writer, SpeechSynthesisAdditions value, JsonSerializerOptions options)
    {
        var json = JsonSerializer.Serialize(value, CreateInnerOptions(options));
        writer.WriteStringValue(json);
    }

    private static JsonSerializerOptions CreateInnerOptions(JsonSerializerOptions options)
    {
        var serializerOptions = new JsonSerializerOptions(options);
        for (var i = serializerOptions.Converters.Count - 1; i >= 0; i--)
        {
            if (serializerOptions.Converters[i] is SpeechSynthesisAdditionsJsonConverter)
            {
                serializerOptions.Converters.RemoveAt(i);
            }
        }

        return serializerOptions;
    }
}
