namespace NcCore.Networking;

/// <summary> Serializer and deserializer of chat messages in JSON format </summary>
public static class NcMessageSerializer
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string Serialize(NcChatMessage message) => JsonSerializer.Serialize(message, _options);

    public static NcChatMessage? Deserialize(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<NcChatMessage>(json, _options);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
