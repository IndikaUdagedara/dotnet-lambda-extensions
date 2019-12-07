
dotnet lambda package --framework netcoreapp2.1 --configuration Release `
    -o $PSScriptRoot\APIGatewayCustomAuthorizer.zip `
    -pl $PSScriptRoot\..\src\APIGatewayCustomAuthorizer
