# setup stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS setup-stage
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore
COPY . .

# development stage
FROM setup-stage as development-stage
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 3002
RUN apt-get update
RUN apt-get install -y unzip
RUN curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l ~/vsdbg
CMD ["dotnet", "watch"]

# build stage
FROM setup-stage as build-stage
RUN dotnet publish -c Release -o dist

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production-stage
WORKDIR /app
COPY --from=build-stage /app/dist ./
EXPOSE 8080
ENTRYPOINT ["dotnet", "Server.API.dll"]