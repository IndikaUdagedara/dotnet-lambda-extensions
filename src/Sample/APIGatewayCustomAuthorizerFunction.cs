using Amazon.Lambda.APIGatewayEvents;
using AWS.Lambda.Extensions;
using Microsoft.Extensions.Hosting;

namespace Sample
{
    public class APIGatewayCustomAuthorizerFunction : AbstractDotNetCoreFunction<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse>
    {
        protected override void Init(IHostBuilder builder)
        {
            builder
                .UseLambda<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse, CustomAuthorizerHandler>();
        }
    }
}