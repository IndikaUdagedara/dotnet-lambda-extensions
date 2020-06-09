using Amazon.Lambda.APIGatewayEvents;
using LambdaExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace APIGatewayCustomAuthorizer
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