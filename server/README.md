# Server

## Project Setup

```sh
dotnet restore
```

### Compile and Run for Development

```sh
dotnet run --project Server.API/Server.API.csproj --watch
```

### Publish for Production

```sh
dotnet publish --project Server.API/Server.API.csproj --configuration Release
```

### Run Tests with [xUnit](https://xunit.net/)

```sh
dotnet test
```

### Format

```sh
dotnet format
```
