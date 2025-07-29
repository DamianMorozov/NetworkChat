namespace NcCore.Networking;

/// <summary> Base class for TCP Worker (server or client) </summary>
public partial class NcTcpWorkerBase : ObservableRecipient, INcTcpWorker
{
    #region Public and private fields, properties, constructor

    /// <inheritdoc />
    [ObservableProperty]
    public partial NcTcpWorkerType TcpWorkerType { get; private set; } = NcTcpWorkerType.Undefined;
    /// <inheritdoc />
    [ObservableProperty]
    public partial TcpClient? Client { get; set; }
    /// <inheritdoc />
    [ObservableProperty]
    public partial NetworkStream? NetworkStream { get; set; }
    /// <inheritdoc />
    [ObservableProperty]
    public partial CancellationTokenSource? Cts { get; private set; }
    /// <inheritdoc />
    [ObservableProperty]
    public partial string IpAddressPort { get; protected set; }
    /// <inheritdoc />
    [ObservableProperty]
    public partial bool IsRunning { get; protected set; }
    /// <inheritdoc />
    [ObservableProperty]
    public partial bool IsConnected { get; protected set; }

    /// <inheritdoc />
    public Action<string>? StatusChanged { get; protected set; }
    /// <inheritdoc />
    public Action<string>? MessageReceived { get; protected set; }
    /// <inheritdoc />
    public Action? ActionStarted { get; protected set; }
    /// <inheritdoc />
    public Action? ActionStopped { get; protected set; }

    public NcTcpWorkerBase() : this(NcTcpWorkerType.Undefined, string.Empty, _ => { }, _ => { }) { }
    
    public NcTcpWorkerBase(NcTcpWorkerType tpcWorkerType, string ipAddressPort, Action<string> statusChanged, Action<string> messageReceived)
    {
        TcpWorkerType = tpcWorkerType;
        IpAddressPort = ipAddressPort;

        StatusChanged += statusChanged;
        MessageReceived += messageReceived;

        switch (TcpWorkerType)
        {
            case NcTcpWorkerType.Server:
                ActionStarted += () =>
                {
                    if (IsRunning)
                        StatusChanged?.Invoke("Server started");
                    if (IsConnected)
                        StatusChanged?.Invoke($"Server connected {IpAddressPort.ExtractIpAddress()}:{IpAddressPort.ExtractPort()}");
                };
                ActionStopped += () => StatusChanged?.Invoke("Server stopped");
                break;
            case NcTcpWorkerType.Client:
                ActionStarted += () =>
                {
                    if (IsRunning)
                        StatusChanged?.Invoke("Client started");
                    if (IsConnected)
                        StatusChanged?.Invoke($"Client connected {IpAddressPort.ExtractIpAddress()}:{IpAddressPort.ExtractPort()}");
                };
                ActionStopped += () => StatusChanged?.Invoke("Client stopped");
                break;
        }
    }

    #endregion

    #region Public and private methods

    /// <inheritdoc />
    public virtual async Task StartAsync()
    {
        CheckIfDisposed();

        Cts = new CancellationTokenSource();
        Client = new TcpClient
        {
            NoDelay = true, // Disable Nagle's algorithm to reduce delays
            ReceiveTimeout = 5_000,
            SendTimeout = 5_000
        };

        if (IsRunning) return;
        IsRunning = true;

        await Task.CompletedTask;
    }

    /// <inheritdoc />
    public virtual void Stop()
    {
        try
        {
            Cts?.Cancel();
            Cts?.Dispose();
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Cancelation Token closing error: {ex}");
        }

        try
        {
            NetworkStream?.Close();
            NetworkStream?.Dispose();
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Network stream closing error: {ex}");
        }

        try
        {
            Client?.Close();
            Client?.Dispose();
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Client closing error: {ex}");
        }

        IsRunning = false;
        IsConnected = false;
        ActionStopped?.Invoke();

        StatusChanged = null;
        MessageReceived = null;
        ActionStarted = null;
        ActionStopped = null;
    }

    /// <inheritdoc />
    public virtual async Task ReceiveLoopAsync(NetworkStream stream, CancellationToken token)
    {
        await Task.Delay(1);
        throw new NotImplementedException($"Method {nameof(ReceiveLoopAsync)} must be implemented in successors {nameof(NcTcpWorkerBase)}");
    }

    /// <inheritdoc />
    public virtual async Task SendMessageAsync(string message)
    {
        await Task.Delay(1);
        throw new NotImplementedException($"Method {nameof(SendMessageAsync)} must be implemented in successors {nameof(NcTcpWorkerBase)}");
    }

    #endregion

    #region Public and private methods - IDisposable

    /// <summary> To detect redundant calls </summary>
    private bool _disposed;

    /// <summary> Finalizer </summary>
    ~NcTcpWorkerBase() => Dispose(false);

    /// <summary> Throw exception if disposed </summary>
    private void CheckIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(NcTcpServer), "The object has already been cleared!");
    }

    /// <summary> Release managed resources </summary>
    private void ReleaseManagedResources()
    {
        //
    }

    /// <summary> Release unmanaged resources </summary>
    private void ReleaseUnmanagedResources()
    {
        Stop();
    }

    /// <summary> Dispose pattern </summary>
    public void Dispose()
    {
        // Release managed resources
        Dispose(disposing: true);
        // Suppress finalization
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        // Release managed resources
        if (disposing)
            ReleaseManagedResources();
        // Release unmanaged resources
        ReleaseUnmanagedResources();
        // Flag
        _disposed = true;
    }

    #endregion
}