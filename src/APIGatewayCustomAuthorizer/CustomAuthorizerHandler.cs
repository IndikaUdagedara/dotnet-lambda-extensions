using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using LambdaExtensions;
using Microsoft.Extensions.Logging;

namespace APIGatewayCustomAuthorizer
{
    public class CustomAuthorizerHandler : IProxyHandler<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse>
    {
        private readonly ILogger<CustomAuthorizerHandler> _logger;

        public CustomAuthorizerHandler(ILogger<CustomAuthorizerHandler> logger)
        {
            _logger = logger;
        }

        public Task<APIGatewayCustomAuthorizerResponse> HandleAsync(APIGatewayCustomAuthorizerRequest request)
        {
            if (request == null)
            {
                return null;
            }

            var response = new APIGatewayCustomAuthorizerResponse
            {
                PrincipalID = request.AuthorizationToken == "good" ? "123" : "0",
                PolicyDocument = new APIGatewayCustomAuthorizerPolicy
                {
                    Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
                    {
                        new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                        {
                            Action = new HashSet<string> {"apigateway:*"},
                            Resource = new HashSet<string> {"*"},
                            Effect = request.AuthorizationToken == "good" ? "Allow" : "Deny"
                        }
                    }
                }
            };

            return Task.FromResult(response);
        }
    }
}