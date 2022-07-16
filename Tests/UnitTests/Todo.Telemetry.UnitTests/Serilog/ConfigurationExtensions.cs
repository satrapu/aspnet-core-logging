namespace Todo.Telemetry.Serilog
{
    using Microsoft.Extensions.Configuration;

    using System.IO;
    using System.Text;

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
