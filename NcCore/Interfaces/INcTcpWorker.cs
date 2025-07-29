namespace NcCore.Interfaces;

/// <summary> TCP Worker </summary>
public interface INcTcpWorker : IDisposable
{
    /// <summary> TCP worker type (server or client) </summary>
    public NcTcpWorkerType TcpWorkerType { get; }
    /// <summary> TCP client or server </summary>
    public TcpClient? Client { get; }
    /// <summary> Flow for reading/writing data </summary>
    public NetworkStream? NetworkStream { get; }
    /// <summary> Token source </summary>
    public CancellationTokenSource? Cts { get; }
    /// <summary> IP address and port </summary>
    public string IpAddressPort { get; }
    /// <summary> Running flag </summary>
    public bool IsRunning { get; }
    /// <summary> Connection flag </summary>
    public bool IsConnected { get; }
    /// <summary> Status change action </summary>
    public Action<string>? StatusChanged { get; }
    /// <summary> Message received action </summary>
    public Action<string>? MessageReceived { get; }
    /// <summary> Started action </summary>
    public Action? ActionStarted { get; }
    /// <summary> Stopped action </summary>
    public Action? ActionStopped { get; }

    /// <summary> Start TCP worker </summary>
    public Task StartAsync();
    /// <summary> Stop TCP worker </summary>
    public void Stop();
    /// <summary> Listening to incoming messages </summary>
    public Task ReceiveLoopAsync(NetworkStream stream, CancellationToken token);
    /// <summary> Send message </summary>
    public Task SendMessageAsync(string message);
}
