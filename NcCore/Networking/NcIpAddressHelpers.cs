namespace NcCore.Networking;

/// <summary> IP Address and port helper </summary>
public static class NcIpAddressHelpers
{
    /// <summary> Get the local IP address of the host </summary>
    public static string GetLocalIpAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("Unable to find local IP address!");
    }

    /// <summary> Extract the IP address </summary>
    public static string ExtractIpAddress(this string ipAddressPort)
    {
        if (string.IsNullOrWhiteSpace(ipAddressPort))
            throw new ArgumentException("IP-address and port cannot be empty!", nameof(ipAddressPort));
        var parts = ipAddressPort.Split(':');
        if (parts.Length < 2)
            throw new FormatException("Incorrect IP address and port format, expected `IP:port`!");
        return parts[0].Trim();
    }

    /// <summary> Extract the port </summary>
    public static int ExtractPort(this string ipAddressPort)
    {
        if (string.IsNullOrWhiteSpace(ipAddressPort))
            throw new ArgumentException("IP-address and port cannot be empty!", nameof(ipAddressPort));
        var parts = ipAddressPort.Split(':');
        if (parts.Length < 2)
            throw new FormatException("Incorrect IP address and port format, expected `IP:port`!");
        var port = parts[1].Trim();
        if (!int.TryParse(port, out int portNumber) || portNumber < 0 || portNumber > 65535)
        {
            throw new FormatException("The port must be a number between 0 and 65535!");
        }
        return portNumber;
    }

    /// <summary> Create IPEndPoint </summary>
    public static IPEndPoint CreateEndPoint(this string ipAddressPort)
    {
        var ip = ExtractIpAddress(ipAddressPort);
        var port = ExtractPort(ipAddressPort);
        return new IPEndPoint(IPAddress.Parse(ip), port);
    }
}
