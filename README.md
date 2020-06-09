# Extensions for aws-lambda-dotnet

[aws-lambda-dotnet](https://github.com/aws/aws-lambda-dotnet) provides a `LambdaServer` to integrate with APIGateway requests.

 This project introduces couple of extension points to connect other types of integrations (SQS, CustomAuthorizer etc.) to .NET application host
 
It provides:

- .NET-idiomatic experience to create lambda function handlers without worrying about host services (configuration, DI, logging) similar to the existing APIGateway integration
- local lambda invocation with an HTTP endpoint to be used with common tools (Postman, curl)
- `sam local` invocation (TODO)

## Usage

1. Create a ASP.NET Core project as you would normally (e.g. `dotnet new webapi`)

2. Implement a function handler by implementing `ILambdaFunctionHandler<TRequest,TResponse>:HandleAsync()` method. `TRequest` is the type of event received from Lambda (e.g. `APIGatewayCustomAuthorizerRequest`) and `TResponse` is the type of response to be returned (e.g. `APIGatewayCustomAuthorizerResponse`)
```
    public class CustomAuthorizerHandler : ILambdaFunctionHandler<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse>
    {
        public Task<APIGatewayCustomAuthorizerResponse> HandleAsync(APIGatewayCustomAuthorizerRequest request)
        { 
            ...
        }
    }
```

3. To configure the `Host`, extend `AbstractDotNetCoreFunction<TRequest, TResponse>` and override the `Init` method and register the handler
```
    public class APIGatewayCustomAuthorizerFunction : AbstractDotNetCoreFunction<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse>
    {
        protected override void Init(IHostBuilder builder)
        {
            builder
                .UseLambda<APIGatewayCustomAuthorizerRequest, APIGatewayCustomAuthorizerResponse, CustomAuthorizerHandler>();
        }
    }
```
4. To be able to run the application locally, create a `GenericHost` with a local lambda server

```
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
```
This will allow to test events with something like following (e.g. simulate `APIGatewayCustomAuthorizerRequest`)
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


5. Lambda handler needs to be configured as

```
<dll>::<Namespace>.<ClassExtendingGenericProxyFunction>::FunctionHandlerAsync
```
e.g. `APIGatewayCustomAuthorizer::APIGatewayCustomAuthorizer.APIGatewayCustomAuthorizerFunction::FunctionHandlerAsync`


## Example

1. To build the sample, run `./deploy/Build.ps1`
2. To deploy, run `./deploy/Deploy.ps1 <artificatsBucket>` where `<artificatsBucket>` is a bucket to store the Lambda code and artifacts.