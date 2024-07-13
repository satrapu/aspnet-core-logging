using System.Diagnostics.CodeAnalysis;

namespace Todo.WebApi.AcceptanceTests.Infrastructure
{
    using System.Net;
    using System.Net.Sockets;

    public class TcpPortProvider
    {
        private static readonly int AvailableTcpPort;

        static TcpPortProvider()
        {
            AvailableTcpPort = InternalGetAvailableTcpPort();
        }

        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Ensure only one TCP port is opened while executing acceptance tests")]
        public int GetAvailableTcpPort()
        {
            return AvailableTcpPort;
        }

        private static int InternalGetAvailableTcpPort()
        {
            using TcpListener tcpListener = new(IPAddress.Loopback, 0);
            tcpListener.Start();

            int availableTcpPort = ((IPEndPoint)tcpListener.LocalEndpoint).Port;

            tcpListener.Stop();
            return availableTcpPort;
        }
    }
}
