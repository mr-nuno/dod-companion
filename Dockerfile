# Single image: the .NET API serves the React SPA from wwwroot (same-origin, no reverse proxy).

# 1. Build the React SPA.
FROM node:24-alpine AS frontend-build
WORKDIR /frontend
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

# 2. Publish the .NET API.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
ARG VERSION=1.0.0
WORKDIR /src
COPY backend/global.json ./
COPY backend/src/ src/
RUN dotnet restore src/DodCompanion.Api/DodCompanion.Api.csproj
RUN dotnet publish src/DodCompanion.Api/DodCompanion.Api.csproj \
    -c Release -o /app --no-restore /p:Version=$VERSION
# Drop the SPA build output into wwwroot so the API serves it as static content.
COPY --from=frontend-build /frontend/dist /app/wwwroot

# 3. Runtime.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=backend-build /app ./

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "DodCompanion.Api.dll"]
