namespace NcCore.Networking;

/// <summary> TCP Client Implementation </summary>
public sealed partial class NcTcpClient(string ipAddressPort, Action<string> statusChanged, Action<string> messageReceived) : 
    NcTcpWorkerBase(NcTcpWorkerType.Client, ipAddressPort, statusChanged, messageReceived)
{

    #region Public and private methods

    /// <inheritdoc />
    public override async Task StartAsync()
    {
        await base.StartAsync();

        try
        {
            if (Client is not null)
            {
                await Client.ConnectAsync(IpAddressPort.CreateEndPoint());
                NetworkStream = Client.GetStream();
            }

            IsConnected = true;
            ActionStarted?.Invoke();

            // Start reading incoming messages
            if (NetworkStream is not null && Cts is not null)
                await ReceiveLoopAsync(NetworkStream, Cts.Token);
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke("Connection error: " + ex.Message);
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
                StatusChanged?.Invoke("The server has disconnected the connection");
                break;
            }

            string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            MessageReceived?.Invoke(msg);
        }

        Stop();
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
            StatusChanged?.Invoke("Message sending error: " + ex.Message);
        }
    }

    /// <inheritdoc />
    public override void Stop()
    {
        base.Stop();
    }

    #endregion
}
