namespace NcCore.Networking;

/// <summary> TCP Server Implementation </summary>
public sealed partial class NcTcpServer : NcTcpWorkerBase
{
    #region Public and private fields, properties, constructor

    private TcpListener? _listener;

    #endregion

    #region Public and private methods

    public NcTcpServer(string ipAddressPort, Action<string> statusChanged, Action<string> messageReceived) : 
        base(NcTcpWorkerType.Server, ipAddressPort, statusChanged, messageReceived)
    {
        //
    }

    /// <inheritdoc />
    public override async Task StartAsync()
    {
        await base.StartAsync();

        _listener = new TcpListener(IpAddressPort.CreateEndPoint());
        _listener.Server.NoDelay = true; // Disable Nagle's algorithm to reduce delays
        _listener.Server.ReceiveTimeout = 5_000;
        _listener.Server.SendTimeout = 5_000;
        _listener.Start();
        
        ActionStarted?.Invoke();

        try
        {
            while (Cts is not null && !Cts.Token.IsCancellationRequested)
            {
                StatusChanged?.Invoke("Waiting for client connection ...");
                Client = await _listener.AcceptTcpClientAsync(Cts.Token);
                StatusChanged?.Invoke($"The client has connected {Client.Client.RemoteEndPoint}");
                IsConnected = true;

                NetworkStream = Client.GetStream();

                // Запускаем слушание клиента
                if (NetworkStream is not null && Cts is not null)
                    await ReceiveLoopAsync(NetworkStream, Cts.Token);
            }
            IsConnected = false;
        }
        catch (OperationCanceledException)
        {
            // Stopped intentionally
            IsConnected = false;
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Server error: {ex.Message}");
            IsConnected = false;
        }
        finally
        {
            Stop();
        }
    }

    /// <inheritdoc />
    public override async Task ReceiveLoopAsync(NetworkStream stream, CancellationToken token)
    {
        byte[] buffer = new byte[4096];

        while (!token.IsCancellationRequested)
        {
            if (!stream.CanRead) break;

            int bytesRead = 0;
            try
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
            }
            catch
            {
                StatusChanged?.Invoke("Connection interrupted");
                break;
            }

            if (bytesRead == 0)
            {
                StatusChanged?.Invoke("The client has disconnected");
                break;
            }

            string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            MessageReceived?.Invoke(msg);
        }
    }

    /// <inheritdoc />
    public override async Task SendMessageAsync(string message)
    {
        if (NetworkStream is null || !NetworkStream.CanWrite) return;

        byte[] data = Encoding.UTF8.GetBytes(message);
        try
        {
            await NetworkStream.WriteAsync(data, 0, data.Length);
            await NetworkStream.FlushAsync();
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Message sending error: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public override void Stop()
    {
        try
        {
            _listener?.Dispose();
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke($"Listener closing error: {ex}");
        }

        base.Stop();
    }

    #endregion
}
