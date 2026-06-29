# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG VERSION=1.0.0
WORKDIR /src

COPY global.json ./
COPY src/ src/

RUN dotnet restore src/DodCompanion.Api/DodCompanion.Api.csproj
RUN dotnet publish src/DodCompanion.Api/DodCompanion.Api.csproj \
    -c Release -o /app --no-restore /p:Version=$VERSION

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DodCompanion.Api.dll"]
