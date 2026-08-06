FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG TARGETARCH
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

RUN dotnet restore ./api/api/src/OweMe.Api/OweMe.Api.csproj -a $TARGETARCH
RUN dotnet restore ./api/client/OweMe.Api.Client/OweMe.Api.Client.csproj -a $TARGETARCH

# Copy remaining source files
COPY api/api/ ./api/api/
COPY api/client/ ./api/client/

WORKDIR /repo/api/api/src/OweMe.Api
RUN dotnet publish "OweMe.Api.csproj" \
    -c "$BUILD_CONFIGURATION" \
    -a "$TARGETARCH" \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM base AS final
USER $APP_UID
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OweMe.Api.dll"]