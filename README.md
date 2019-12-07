# Extensions for aws-lambda-dotnet

[aws-lambda-dotnet](https://github.com/aws/aws-lambda-dotnet) provides a `LambdaServer` to integrate with APIGateway requests.

This `LambdaServer` can be used to simplify integrating with other Lambda integrations (e.g. SQS, LambdaAuthorizer). This project introduces extensions and generic methods to connect these integrations with ASP.NET Core pipeline to allow
- using ASP.NET utilities (configuration, DI, logging) to be used seamlessly (as is possible with APIGateway integration)
- local testing e.g. hit an API endpoint and pass integration requests

## Usage

1. Create a ASP.NET Core project as you would normally (e.g. `dotnet new webapi`)

2. Create a class inheriting from `GenericProxyFunction<TRequest, TResponse>`. `TRequest` is the type of the integration event (e.g. `APIGatewayCustomAuthorizerRequest`) and `TResponse` is response type for the integration (e.g. `APIGatewayCustomAuthorizerResponse`) and override the `Init` method to configure the `WebHost`

```
    public class APIGatewayCustomAuthorizerFunction : GenericProxyFunction<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse>
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
```

3. Implement a handler extending from `IProxyHandler<TRequest, TResponse>` to handle the request and return a response.

```
    public class CustomAuthorizerHandler : IProxyHandler<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse>
    {
        private readonly ILogger<CustomAuthorizerHandler> _logger;

        public CustomAuthorizerHandler(ILogger<CustomAuthorizerHandler> logger)
        {
            _logger = logger;
        }

        public Task<APIGatewayCustomAuthorizerResponse> HandleAsync(APIGatewayCustomAuthorizerRequest request)
        {
            // Implement request handling
            // ...
        }
    }
```

4. In the `Startup` class add the handler to the services. Note `.AddMvc()`. Not all of `Mvc` is required. Optimize with `.AddMvcCore()` as necessary. 

```
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
        // ... 
        services.AddScoped<IProxyHandler<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse>,         CustomAuthorizerHandler>();
    }
```

5. In the `Startup.Configure()` method, instead of `.UseMvc()` use the lambda proxy
```
    public void Configure(IApplicationBuilder app, IProxyHandler<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse> handler)
    {
        app.UseLambdaProxy(handler);
    }
```
This where the _magic_ happens i.e. the integration event is passed to the handler via the ASP.NET request pipeline enabling the same features as with the `APIGatewayRequest` integration (`APIGatewayProxyFunction`) and also ability to locally start the application as a webserver and pass integration events as http requests.

To make an `APIGatewayCustomAuthorizerRequest` following can be used
```
curl -X POST http://localhost:5000/ -d \
'{
    "type": "aaa",
    "authorizationToken": "bbb",
    "methodArn": "ccc",
    "path": "ddd",
    "httpMethod": "POST",
    "headers": null,
    "queryStringParameters": null,
    "pathParameters": null,
    "stageVariables": null
}'
```

which should return
```
{
    "principalId": "1111",
    "policyDocument": {
        "Version": "2012-10-17",
        "Statement": [
            {
                "Effect": "Allow",
                "Action": [
                    "apigateway:*"
                ],
                "Resource": [
                    "*"
                ]
            }
        ]
    },
    "context": null,
    "usageIdentifierKey": null
}
