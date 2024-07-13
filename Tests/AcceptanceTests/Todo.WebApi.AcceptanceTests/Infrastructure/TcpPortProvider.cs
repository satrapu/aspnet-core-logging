namespace Todo.WebApi.AcceptanceTests.Infrastructure
{
    using System.Net;
    using System.Net.Sockets;

    public class TcpPortProvider
    {
        private readonly int availableTcpPort = InternalGetAvailableTcpPort();

        public int GetAvailableTcpPort() => availableTcpPort;

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
