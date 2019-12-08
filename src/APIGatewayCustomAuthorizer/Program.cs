using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace APIGatewayCustomAuthorizer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME")))
            {
                CreateWebHostBuilder(args)
                    .Build()
                    .Run();
            }
            else
            {
                var lambdaEntry = new APIGatewayCustomAuthorizerFunction();
                var functionHandler = (Func<Stream, ILambdaContext, Task<Stream>>) lambdaEntry
                    .FunctionHandlerAsync;
                using (var handlerWrapper = HandlerWrapper.GetHandlerWrapper(functionHandler))
                using (var bootstrap = new LambdaBootstrap(handlerWrapper))
                {
                    await bootstrap.RunAsync();
                }
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .UseStartup<Startup>()
                .UseKestrel();
        }
    }
}