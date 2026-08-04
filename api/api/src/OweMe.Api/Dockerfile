FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /repo

# Copy only project files first for better layer caching
COPY version.json ./
COPY api/version.json ./api/
COPY api/Directory.Build.props ./
COPY api/Directory.Packages.props ./
COPY api/OweMe.Api.sln ./api/

# Copy all .csproj files to restore dependencies
COPY api/api/src/OweMe.Api/*.csproj ./api/api/src/OweMe.Api/
COPY api/api/src/OweMe.Application/*.csproj ./api/api/src/OweMe.Application/
COPY api/api/src/OweMe.Domain/*.csproj ./api/api/src/OweMe.Domain/
COPY api/api/src/OweMe.Infrastructure/*.csproj ./api/api/src/OweMe.Infrastructure/
COPY api/api/src/OweMe.Persistence/*.csproj ./api/api/src/OweMe.Persistence/
COPY api/client/OweMe.Api.Client/*.csproj ./api/client/OweMe.Api.Client/

# Restore only the main project dependencies (excluding test projects)
RUN dotnet restore ./api/api/src/OweMe.Api/OweMe.Api.csproj
RUN dotnet restore ./api/client/OweMe.Api.Client/OweMe.Api.Client.csproj

# Copy remaining source files
COPY api/api/ ./api/api/
COPY api/client/ ./api/client/

# Build the project
WORKDIR /repo/api/api/src/OweMe.Api
RUN dotnet build "OweMe.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OweMe.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OweMe.Api.dll"]
