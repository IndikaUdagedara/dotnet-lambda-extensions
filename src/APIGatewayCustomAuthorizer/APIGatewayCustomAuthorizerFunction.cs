using LambdaExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace APIGatewayCustomAuthorizer
{
    public class APIGatewayCustomAuthorizerFunction : GenericProxyFunction
    {
        protected override void Init(IWebHostBuilder builder)
        {
            builder
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .UseStartup<Startup>()
                .UseLambdaServer();
        }
    }
}