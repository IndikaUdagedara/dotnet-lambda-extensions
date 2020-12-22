rm -r publish/
dotnet build -o publish/
pushd publish
dotnet nuget push *.nupkg -k oy2oxerznwpsmnm7bbequqdbmdd3cgauxgtivvgzpjkeh4 -s https://api.nuget.org/v3/index.json
popd