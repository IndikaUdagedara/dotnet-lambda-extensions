using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;

namespace APIGatewayCustomAuthorizer.Tests
{
    public class LocalServerTests
        : IClassFixture<WebApplicationFactory<Startup>>
    {
        private readonly WebApplicationFactory<Startup> _factory;

        public LocalServerTests(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("good", "123")]
        [InlineData("bad", "0")]
        public async Task HttpRequest_Should_Return_Valid_Response(string token, string principalId)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var authRequest = new APIGatewayCustomAuthorizerRequest()
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
            var function = new APIGateweyAuthorizerFunction();

            // Act
            var authRequest = new APIGatewayCustomAuthorizerRequest()
            {
                AuthorizationToken = token
            };

            var authResponse = await function.FunctionHandlerAsync(authRequest, new TestLambdaContext());

            // Assert
            authResponse.PrincipalID.Should().Be(principalId);
        }
    }
}
