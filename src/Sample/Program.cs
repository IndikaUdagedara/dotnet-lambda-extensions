using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using AWS.Lambda.Extensions;
using Microsoft.Extensions.Hosting;

namespace Sample
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseLambdaLocal<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse,
                    CustomAuthorizerHandler>();
        }


    }
}