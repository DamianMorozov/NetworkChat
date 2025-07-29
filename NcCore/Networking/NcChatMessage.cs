namespace NcCore.Networking;

/// <summary> Chat message for TCP Worker </summary>
public sealed class NcChatMessage
{
    [JsonPropertyName("type")]
    public NcMessageType Type { get; set; }

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("sender")]
    public string Sender { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    public NcChatMessage()
    {
        Timestamp = DateTime.UtcNow;
    }

    public NcChatMessage(NcMessageType type, string content, string sender) : this()
    {
        Type = type;
        Content = content;
        Sender = sender;
    }
}
