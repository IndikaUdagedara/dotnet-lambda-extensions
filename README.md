# Extensions for aws-lambda-dotnet

[aws-lambda-dotnet](https://github.com/aws/aws-lambda-dotnet) provides a `LambdaServer` to integrate with APIGateway requests.

This `LambdaServer` can be used to simplify integrating with other Lambda integrations (e.g. SQS, LambdaAuthorizer). This project introduces extensions and generic methods to connect these integrations with ASP.NET Core pipeline to allow
- local testing e.g. hit an API endpoint and pass integration requests
- using ASP.NET utilities (configuration, DI, logging) to be used seemlessly (as is possible with APIGateway integration)