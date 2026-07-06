# MCP server: a stateless SSE proxy over the external DoD Rules API.

# 1. Publish the .NET app.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION=1.0.0
WORKDIR /src
COPY global.json ./
COPY DodMcpServer.csproj ./
RUN dotnet restore DodMcpServer.csproj
COPY . ./
RUN dotnet publish DodMcpServer.csproj -c Release -o /app --no-restore /p:Version=$VERSION

# 2. Runtime.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DodMcpServer.dll"]
