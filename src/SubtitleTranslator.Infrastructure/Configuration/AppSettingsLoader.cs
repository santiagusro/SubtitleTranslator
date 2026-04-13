using System.Text.Json;
using System.Text.Json.Serialization;
using SubtitleTranslator.Core.Models;

namespace SubtitleTranslator.Infrastructure.Configuration;

public sealed class AppSettingsLoader
{
    private static readonly string SettingsFilePath =
        Path.Combine(AppContext.BaseDirectory, "appsettings.json");

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        Converters = { new CaptureRegionJsonConverter() }
    };

    public AppSettings Load()
    {
        if (!File.Exists(SettingsFilePath))
            return new AppSettings();

        try
        {
            string json = File.ReadAllText(SettingsFilePath);
            return JsonSerializer.Deserialize<AppSettings>(json, SerializerOptions) ?? new AppSettings();
        }
        catch (JsonException)
        {
            // Archivo corrupto → devolver configuración por defecto
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        string json = JsonSerializer.Serialize(settings, SerializerOptions);
        File.WriteAllText(SettingsFilePath, json);
    }

    /// <summary>
    /// Converter necesario porque CaptureRegion tiene un constructor privado y usa
    /// un factory method (CaptureRegion.Create) que valida las dimensiones.
    /// </summary>
    private sealed class CaptureRegionJsonConverter : JsonConverter<CaptureRegion?>
    {
        public override CaptureRegion? Read(
            ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            int x = 0, y = 0, width = 0, height = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                string propertyName = reader.GetString()!;
                reader.Read();

                switch (propertyName.ToUpperInvariant())
                {
                    case "X":      x      = reader.GetInt32(); break;
                    case "Y":      y      = reader.GetInt32(); break;
                    case "WIDTH":  width  = reader.GetInt32(); break;
                    case "HEIGHT": height = reader.GetInt32(); break;
                }
            }

            if (width <= 0 || height <= 0)
                return null;

            return CaptureRegion.Create(x, y, width, height);
        }

        public override void Write(
            Utf8JsonWriter writer, CaptureRegion? value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Width", value.Width);
            writer.WriteNumber("Height", value.Height);
            writer.WriteEndObject();
        }
    }
}
