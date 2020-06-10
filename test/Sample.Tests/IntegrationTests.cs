using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Xunit;

namespace Sample.Tests
{
    public class LocalServerTests
    {
        [Theory]
        [InlineData("good", "123")]
        [InlineData("bad", "0")]
        public async Task HttpRequest_Should_Return_Valid_Response(string token, string principalId)
        {
            // Arrange
            var hostBuilder = Program.CreateHostBuilder(new string[] {})
                .ConfigureWebHost(webHost =>
                {
                    webHost.UseTestServer();
                });


            var host = await hostBuilder.StartAsync();
            var client = host.GetTestClient();

            // Act
            var authRequest = new APIGatewayCustomAuthorizerRequest
            {
                AuthorizationToken = token
            };

            var response = await client.PostAsync("/",
                new StringContent(JsonConvert.SerializeObject(authRequest), Encoding.UTF8, "application/json"));

            // Assert
            response.EnsureSuccessStatusCode();
            response.Content.Headers.ContentType.ToString().Should().Be("application/json");

            var authResponse =
                JsonConvert.DeserializeObject<APIGatewayCustomAuthorizerResponse>(
                    await response.Content.ReadAsStringAsync());

            authResponse.Should().NotBeNull();
            authResponse.PrincipalID.Should().Be(principalId);
        }
    }

    public class LambdaTests
    {
        [Theory]
        [InlineData("good", "123")]
        [InlineData("bad", "0")]
        public async Task FunctionInvoke_Should_Return_Valid_Response(string token, string principalId)
        {
            // Arrange
            var function = new APIGatewayCustomAuthorizerFunction();

            // Act
            var authRequest = new APIGatewayCustomAuthorizerRequest
            {
                AuthorizationToken = token
            };

            var response = await function.FunctionHandlerAsync(authRequest, new TestLambdaContext());

            // Assert
            response.PrincipalID.Should().Be(principalId);
        }
    }
}