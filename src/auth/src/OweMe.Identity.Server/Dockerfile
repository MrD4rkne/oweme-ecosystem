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
COPY src/auth/version.json ./src/auth/
COPY src/auth/Directory.Build.props ./
COPY src/auth/Directory.Packages.props ./
COPY src/auth/OweMe.Identity.sln ./src/auth/

# Copy all .csproj files to restore dependencies
COPY src/auth/src/OweMe.Identity.Server/*.csproj ./src/auth/src/OweMe.Identity.Server/
COPY src/auth/src/OweMe.Identity.Persistence/*.csproj ./src/auth/src/OweMe.Identity.Persistence/
COPY src/auth/src/OweMe.Identity.Migrator/*.csproj ./src/auth/src/OweMe.Identity.Migrator/

# Restore dependencies (cached layer unless csproj changes)
RUN dotnet restore ./src/auth/src/OweMe.Identity.Server/OweMe.Identity.Server.csproj
RUN dotnet restore ./src/auth/src/OweMe.Identity.Migrator/OweMe.Identity.Migrator.csproj

# Copy remaining source files
COPY src/auth/src/ ./src/auth/src/

# Build the projects
WORKDIR /repo/src/auth/src/OweMe.Identity.Server
RUN dotnet build "OweMe.Identity.Server.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

WORKDIR /repo/src/auth/src/OweMe.Identity.Migrator
RUN dotnet build "OweMe.Identity.Migrator.csproj" -c "$BUILD_CONFIGURATION" -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

# Publish Web App to its own temporary folder
WORKDIR /repo/src/auth/src/OweMe.Identity.Server
RUN dotnet publish "OweMe.Identity.Server.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish/server \
    /p:UseAppHost=false

# Publish Migrator to its own temporary folder
WORKDIR /repo/src/auth/src/OweMe.Identity.Migrator
RUN dotnet publish "OweMe.Identity.Migrator.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -o /app/publish/migrator \
    /p:UseAppHost=false

FROM base AS final
USER $APP_UID
WORKDIR /app

COPY --from=publish /app/publish/server ./server
COPY --from=publish /app/publish/migrator ./migrator

WORKDIR /app/server
ENTRYPOINT ["dotnet", "OweMe.Identity.Server.dll"]
