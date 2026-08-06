FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release
WORKDIR /repo

# Copy layer cache dependencies
COPY version.json ./
COPY auth/version.json ./auth/
COPY auth/Directory.Build.props ./
COPY auth/Directory.Packages.props ./
COPY auth/OweMe.Identity.sln ./auth/

# Copy all .csproj files
COPY auth/src/OweMe.Identity.Server/*.csproj ./auth/src/OweMe.Identity.Server/
COPY auth/src/OweMe.Identity.Persistence/*.csproj ./auth/src/OweMe.Identity.Persistence/
COPY auth/src/OweMe.Identity.Migrator/*.csproj ./auth/src/OweMe.Identity.Migrator/

RUN dotnet restore ./auth/src/OweMe.Identity.Server/OweMe.Identity.Server.csproj -a $TARGETARCH
RUN dotnet restore ./auth/src/OweMe.Identity.Migrator/OweMe.Identity.Migrator.csproj -a $TARGETARCH

# Copy remaining source files
COPY auth/src/ ./auth/src/

WORKDIR /repo/auth/src/OweMe.Identity.Server
RUN dotnet publish "OweMe.Identity.Server.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -a "$TARGETARCH" \
    -o /app/publish/server \
    --no-restore \
    /p:UseAppHost=false

WORKDIR /repo/auth/src/OweMe.Identity.Migrator
RUN dotnet publish "OweMe.Identity.Migrator.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -a "$TARGETARCH" \
    -o /app/publish/migrator \
    --no-restore \
    /p:UseAppHost=false

FROM base AS final
USER $APP_UID
WORKDIR /app

COPY --from=build /app/publish/server ./server
COPY --from=build /app/publish/migrator ./migrator

WORKDIR /app/server
ENTRYPOINT ["dotnet", "OweMe.Identity.Server.dll"]