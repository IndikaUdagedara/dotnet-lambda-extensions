# Extensions for aws-lambda-dotnet

[aws-lambda-dotnet](https://github.com/aws/aws-lambda-dotnet) provides a `LambdaServer` to integrate with APIGateway requests.

 This project introduces couple of extension points to connect other types of integrations (SQS, CustomAuthorizer etc.) to the `LambdaServer` and thereby integrating with the ASP.NET Core http pipeline. The benefits are:
 
- ability to use ASP.NET utilities (configuration, DI, logging) seamlessly (as is possible with APIGateway integration)
- local exploratory testing e.g. hit an API endpoint and pass integration requests

## Usage

1. Create a ASP.NET Core project as you would normally (e.g. `dotnet new webapi`)

2. Create a class inheriting `GenericProxyFunction<TRequest, TResponse>`. `TRequest` is the type of the integration event (e.g. `APIGatewayCustomAuthorizerRequest`) and `TResponse` is response type for the integration (e.g. `APIGatewayCustomAuthorizerResponse`) and override the `Init` method to configure the `WebHost`

```
    public class APIGatewayCustomAuthorizerFunction : GenericProxyFunction<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse>
    {
        protected override void Init(IWebHostBuilder builder)
        {
            builder
                // .Use...()
                .UseStartup<Startup>()
                .UseLambdaServer();
        }
    }
```

3. Implement a handler extending `IProxyHandler<TRequest, TResponse>` to handle the request and return a response.

```
    public class CustomAuthorizerHandler : IProxyHandler<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse>
    {
        // ...

        public Task<APIGatewayCustomAuthorizerResponse> HandleAsync(APIGatewayCustomAuthorizerRequest request)
        {
            // Implement request handling
            // ...
        }
    }
```

4. In the `Startup` class add the handler to the services. Note: not all of MVC (`.AddMvc()`) is required

```
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddMvcCore()
            .AddApiExplorer()
            .AddJsonFormatters();

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
This is where the _magic_ happens i.e. the integration event is passed to the handler via the ASP.NET request pipeline enabling the same features as with the `APIGatewayRequest` integration (`APIGatewayProxyFunction`) and also ability to locally start the application as a webserver and pass integration events as http requests.

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


5. Set the handler in Lambda to

```
<dll>::<Namespace>.<ClassExtendingGenericProxyFunction>::FunctionHandlerAsync
```
e.g. `APIGatewayCustomAuthorizer::APIGatewayCustomAuthorizer.APIGatewayCustomAuthorizerFunction::FunctionHandlerAsync`


## Example

1. To build the sample, run `./deploy/Build.ps1`
2. To deploy, run `./deploy/Deploy.ps1 <artificatsBucket>` where `<artificatsBucket>` is a bucket to store the Lambda code and artifacts.