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
COPY src/api/version.json ./src/api/
COPY src/api/Directory.Build.props ./
COPY src/api/Directory.Packages.props ./
COPY src/api/OweMe.Api.sln ./src/api/

# Copy all .csproj files to restore dependencies
COPY src/api/api/src/OweMe.Api/*.csproj ./src/api/api/src/OweMe.Api/
COPY src/api/api/src/OweMe.Application/*.csproj ./src/api/api/src/OweMe.Application/
COPY src/api/api/src/OweMe.Domain/*.csproj ./src/api/api/src/OweMe.Domain/
COPY src/api/api/src/OweMe.Infrastructure/*.csproj ./src/api/api/src/OweMe.Infrastructure/
COPY src/api/api/src/OweMe.Persistence/*.csproj ./src/api/api/src/OweMe.Persistence/
COPY src/api/client/OweMe.Api.Client/*.csproj ./src/api/client/OweMe.Api.Client/

# Restore only the main project dependencies (excluding test projects)
RUN dotnet restore ./src/api/api/src/OweMe.Api/OweMe.Api.csproj
RUN dotnet restore ./src/api/client/OweMe.Api.Client/OweMe.Api.Client.csproj

# Copy remaining source files
COPY src/api/api/ ./src/api/api/
COPY src/api/client/ ./src/api/client/

# Build the project
WORKDIR /repo/src/api/api/src/OweMe.Api
RUN dotnet build "OweMe.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OweMe.Api.csproj" -c "$BUILD_CONFIGURATION" -o /app/publish /p:UseAppHost=false

FROM base AS final
USER $APP_UID
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OweMe.Api.dll"]
