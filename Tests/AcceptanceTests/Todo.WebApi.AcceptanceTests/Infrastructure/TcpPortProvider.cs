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

        public int GetAvailableTcpPort() => AvailableTcpPort;

        private static int InternalGetAvailableTcpPort()
        {
            TcpListener tcpListener = new(IPAddress.Loopback, 0);
            tcpListener.Start();

            int availableTcpPort = ((IPEndPoint)tcpListener.LocalEndpoint).Port;

            tcpListener.Stop();
            return availableTcpPort;
        }
    }
}
