# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Ravana_Astrology is an ASP.NET Core 10.0 Web API project. This is a minimal API setup using the standard ASP.NET Core template with OpenAPI/Swagger support.

**Target Framework:** .NET 10.0
**Project Type:** ASP.NET Core Web API
**Namespace:** Ravana_Astrology

## Build and Run Commands

```bash
# Build the project
dotnet build

# Run the application (development mode with HTTPS)
dotnet run --project Ravana_Astrology/Ravana_Astrology.csproj

# Run with specific profile
dotnet run --project Ravana_Astrology/Ravana_Astrology.csproj --launch-profile https
dotnet run --project Ravana_Astrology/Ravana_Astrology.csproj --launch-profile http

# Clean build artifacts
dotnet clean

# Restore NuGet packages
dotnet restore
```

**Application URLs:**
- HTTPS: https://localhost:7088
- HTTP: http://localhost:5188
- OpenAPI endpoint (dev only): `/openapi/v1.json`

## Architecture

### Project Structure

```
Ravana_Astrology/
├── Controllers/          # API controllers
├── Properties/
│   └── launchSettings.json  # Launch profiles and environment config
├── Program.cs           # Application entry point and middleware configuration
├── appsettings.json     # Application configuration
└── Ravana_Astrology.csproj  # Project file
```

### Key Architecture Points

1. **Entry Point:** `Program.cs` uses minimal hosting model with top-level statements
2. **Controller-based API:** Uses `[ApiController]` attribute with conventional routing
3. **Middleware Pipeline:**
   - Controllers via `AddControllers()` and `MapControllers()`
   - OpenAPI in development environment only
   - HTTPS redirection enabled
   - Authorization middleware configured (but no auth scheme set up yet)

4. **Configuration:**
   - `Nullable` reference types enabled
   - `ImplicitUsings` enabled for C# 10+ features

### Current Dependencies

- `Microsoft.AspNetCore.OpenApi` (v10.0.1) - OpenAPI/Swagger document generation

## Development Notes

### Adding New Controllers

Controllers should:
- Be placed in the `Controllers/` directory
- Inherit from `ControllerBase`
- Use `[ApiController]` attribute
- Use `[Route("[controller]")]` or custom route attribute
- Be in the `Ravana_Astrology.Controllers` namespace

### Configuration

- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- Default logging: Information level, with ASP.NET Core at Warning level
