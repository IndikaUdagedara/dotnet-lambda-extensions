rm -r publish/
dotnet build -o publish/
pushd publish
dotnet nuget push *.nupkg -k x -s https://api.nuget.org/v3/index.json
popd