# OweMe Ecosystem
Holds setup for composing up OweMe application system.

## 🏗️ Building Docker Images

The ecosystem consists of two main services that need to be built as Docker images:
- **oweme.identity** - Identity Server
- **oweme.api** - API Server

### Building images

If you don't access to the images, just run

> docker-compose -f .\compose.build.yaml build

## 🚀 Running the Ecosystem

### Production Mode (Using Pre-built Images)
```bash
docker-compose up
```

### Development Mode (With Live Building)

```bash
# Build and run everything
docker-compose -f compose.yaml -f compose.build.yaml up --build

# Or run individual services in debug mode
cd owe-me-identityserver
docker-compose -f ../compose.yaml -f compose.override.yaml up --build

cd ../owe-me-api  
docker-compose -f ../compose.yaml -f compose.override.yaml up --build
```

## 📊 Services and Ports

| Service | Port | Description |
|---------|------|-------------|
| Identity Server | 5010 (HTTP), 5011 (HTTPS) | Authentication & Authorization |
| API Server | 5000 (HTTP), 5001 (HTTPS) | Main API |
| Seq Logging | 5341 | Centralized logging dashboard |
| Identity DB | 5432 | PostgreSQL for Identity Server |
| API DB | 5442 | PostgreSQL for API Server |

## 🔧 Development Setup

The ecosystem uses Docker Compose override files for development:
- `compose.yaml` - Production configuration (uses pre-built images)
- `compose.override.yaml` - Development configuration (builds from source)

This allows you to easily switch between production and development modes.
