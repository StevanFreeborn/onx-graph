# setup stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS setup-stage
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . .

# development stage
FROM setup-stage as development-stage
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 3000
CMD ["dotnet", "watch"]

# build stage
FROM setup-stage as build-stage
RUN dotnet publish -c Release -o dist

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build-stage /app/dist ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "Server.API.dll"]