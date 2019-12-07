using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using LambdaExtensions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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

            _logger.LogInformation("Request: {0}", JsonConvert.SerializeObject(request));

            var policy = new APIGatewayCustomAuthorizerPolicy
            {
                Version = "2012-10-17",
                Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>()
            };

            policy.Statement.Add(new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
            {
                Action = new HashSet<string>(new[] { "execute-api:Invoke" }),
                Effect = request.AuthorizationToken == "good" ? "Allow" : "Deny",
                Resource = new HashSet<string>(new[] { request.MethodArn })
            });


            var response =  new APIGatewayCustomAuthorizerResponse
            {
                PrincipalID = request.AuthorizationToken == "good" ? "123" : "0",
                PolicyDocument = policy
            };

            _logger.LogInformation("Response: {0}", JsonConvert.SerializeObject(response));

            return Task.FromResult(response);
        }
    }
}