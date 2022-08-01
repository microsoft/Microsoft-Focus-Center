using Microsoft.ApplicationInsights.Channel;
using Microsoft.Extensions.Logging;

namespace AnonymizationFunctionApp
{
    public static class CommonExtensions
    {
        public static void Flush(this ILogger log)
        {
            using var channel = new InMemoryChannel();
            channel.Flush();
        }
    }
}
