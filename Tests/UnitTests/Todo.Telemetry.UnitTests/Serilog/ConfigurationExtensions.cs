namespace Todo.Telemetry.Serilog
{
    using System.IO;
    using System.Text;

    using Microsoft.Extensions.Configuration;

    internal static class ConfigurationExtensions
    {
        internal static IConfiguration ToConfiguration(this string jsonFragment)
        {
            using Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonFragment));

            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.AddJsonStream(stream);
            return configurationBuilder.Build();
        }
    }
}
