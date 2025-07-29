namespace NcCore.ViewModels;

/// <summary> Main ViewModel </summary>
public sealed partial class NcMainViewModel : ObservableRecipient
{
    #region Public and private fields, properties, constructor

    /// <summary> TCP worker type </summary>
    [ObservableProperty]
    public partial NcTcpWorkerType TcpWorkerType { get; private set; }
    /// <summary> TCP worker </summary>
    [ObservableProperty]
    public partial INcTcpWorker? TcpWorker { get; private set; } = default!;
    [ObservableProperty]
    public partial string IpAddressPort { get; set; } = "127.0.0.1:12345";
    [ObservableProperty]
    public partial bool IsServerControlsVisible { get; set; }
    [ObservableProperty]
    public partial bool IsClientControlsVisible { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<string> Messages { get; private set; } = new();
    [ObservableProperty]
    public partial string InputMessage { get; set; } = string.Empty;

    public IAsyncRelayCommand ServerConnectCommand { get; }
    public IAsyncRelayCommand ClientConnectCommand { get; }
    public IRelayCommand ServerDisconnectCommand { get; }
    public IRelayCommand ClientDisconnectCommand { get; }
    public IAsyncRelayCommand SendMessageCommand { get; }
    public IAsyncRelayCommand ClearCommand { get; }

    public NcMainViewModel()
    {
        TcpWorkerType = NcTcpWorkerType.Server;
        TcpWorker = new NcTcpWorkerBase();
        ServerConnectCommand = new AsyncRelayCommand(ServerConnectAsync, () => true);
        ServerDisconnectCommand = new RelayCommand(ServerDisconnect, () => true);
        ClientConnectCommand = new AsyncRelayCommand(ClientConnectAsync, () => true);
        ClientDisconnectCommand = new RelayCommand(ClientDisconnect, () => true);
        SendMessageCommand = new AsyncRelayCommand(SendMessageAsync, () => true);
        ClearCommand = new AsyncRelayCommand(ClearMessagesAsync, () => true);

        // PropertyChanged
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(TcpWorker.IsConnected))
            {
                switch (TcpWorkerType)
                {
                    case NcTcpWorkerType.Server:
                        ServerConnectCommand.NotifyCanExecuteChanged();
                        ServerDisconnectCommand.NotifyCanExecuteChanged();
                        break;
                    case NcTcpWorkerType.Client:
                        ClientConnectCommand.NotifyCanExecuteChanged();
                        ClientDisconnectCommand.NotifyCanExecuteChanged();
                        break;
                }
                SendMessageCommand.NotifyCanExecuteChanged();
                IsClientControlsVisible = TcpWorker?.IsConnected ?? false;
            }
            // Enter message
            else if (e.PropertyName == nameof(InputMessage))
            {
                SendMessageCommand.NotifyCanExecuteChanged();
            }
            // Eliminate ambiguity
            else if (e.PropertyName == nameof(IsServerControlsVisible))
            {
                if (IsServerControlsVisible && IsClientControlsVisible)
                {
                    IsClientControlsVisible = false;
                }
            }
            else if (e.PropertyName == nameof(IsClientControlsVisible))
            {
                if (IsClientControlsVisible && IsServerControlsVisible)
                {
                    IsServerControlsVisible = false;
                }
            }
        };

        IsClientControlsVisible = false;
        IsServerControlsVisible = true;
    }

    ~NcMainViewModel()
    {
        ClientDisconnect();
        ServerDisconnect();
        TcpWorker?.Dispose();
    }

    #endregion

    #region Public and private methods

    /// <summary> Start TCP worker </summary>
    private async Task StartWorkerCoreAsync(NcTcpWorkerType tcpWorkerType)
    {
        await ClearMessagesAsync();
        StopWorkerCore(tcpWorkerType);

        try
        {
            TcpWorkerType = tcpWorkerType;
            switch (TcpWorkerType)
            {
                case NcTcpWorkerType.Server:
                    TcpWorker = new NcTcpServer(IpAddressPort, StatusChanged, MessageReceived);
                    await TcpWorker.StartAsync();
                    break;
                case NcTcpWorkerType.Client:
                    TcpWorker = new NcTcpClient(IpAddressPort, StatusChanged, MessageReceived);
                    await TcpWorker.StartAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            switch (TcpWorkerType)
            {
                case NcTcpWorkerType.Server:
                    StatusChanged($"Server error: {ex.Message}");
                    break;
                case NcTcpWorkerType.Client:
                    StatusChanged($"Client error: {ex.Message}");
                    break;
            }
        }
    }

    /// <summary> Start TCP server </summary>
    private async Task ServerConnectAsync() => await StartWorkerCoreAsync(NcTcpWorkerType.Server);

    /// <summary> Start TCP client </summary>
    private async Task ClientConnectAsync() => await StartWorkerCoreAsync(NcTcpWorkerType.Client);

    /// <summary> Stop TCP worker </summary>
    private void StopWorkerCore(NcTcpWorkerType tcpWorkerType)
    {
        TcpWorkerType = tcpWorkerType;
        TcpWorker?.Stop();
        TcpWorker?.Dispose();
        TcpWorker = null;
    }

    /// <summary> Stop TCP server </summary>
    private void ServerDisconnect() => StopWorkerCore(NcTcpWorkerType.Server);

    /// <summary> Stop TCP client </summary>
    private void ClientDisconnect() => StopWorkerCore(NcTcpWorkerType.Client);

    /// <summary> Send message </summary>
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(InputMessage)) return;
        if (TcpWorker is null) return;

        var sender = TcpWorkerType switch
        {
            NcTcpWorkerType.Server => "Server",
            NcTcpWorkerType.Client => "Client",
            _ => "Unknown"
        };
        var msg = new NcChatMessage(NcMessageType.Message, InputMessage.Trim(), sender);
        var json = NcMessageSerializer.Serialize(msg);

        try
        {
            switch (TcpWorkerType)
            {
                case NcTcpWorkerType.Server:
                    if (TcpWorker.IsConnected)
                        await TcpWorker.SendMessageAsync(json);
                    break;
                case NcTcpWorkerType.Client:
                    if (TcpWorker.IsConnected)
                        await TcpWorker.SendMessageAsync(json);
                    break;
            }
        }
        catch (Exception ex)
        {
            StatusChanged($"Sending error: {ex.Message}");
        }

        AddChatMessage($"You: {InputMessage.Trim()}");
        InputMessage = string.Empty;
    }

    private void MessageReceived(string json)
    {
        var msg = NcMessageSerializer.Deserialize(json);
        if (msg is null)
        {
            StatusChanged("Incorrect message received");
            return;
        }

        switch (msg.Type)
        {
            case NcMessageType.Message:
                AddChatMessage($"{msg.Sender}: {msg.Content}");
                break;
            case NcMessageType.Connect:
            case NcMessageType.Disconnect:
            case NcMessageType.Notification:
                StatusChanged($"[System]: {msg.Content}");
                break;
        }
    }

    private void AddChatMessage(string text) => Messages.Add(text);

    private void StatusChanged(string text) => Messages.Add($"[System]: {text}");

    /// <summary> Clear chat messages </summary>
    private async Task ClearMessagesAsync()
    {
        Messages.Clear();
        await Task.CompletedTask;
    }

    #endregion
}
