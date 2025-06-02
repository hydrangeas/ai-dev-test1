# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Test Commands

This is a .NET 8.0 WPF application with the following common commands:

```bash
# Build the entire solution
dotnet build

# Build specific projects
dotnet build AiDevTest1.WpfApp/AiDevTest1.WpfApp.csproj
dotnet build AiDevTest1.Tests/AiDevTest1.Tests.csproj

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run a specific test class
dotnet test --filter "ClassName=LogEntryFactoryTests"

# Run a specific test method
dotnet test --filter "MethodName=CreateLogEntry_ShouldReturnValidLogEntry"

# Run the WPF application
dotnet run --project AiDevTest1.WpfApp

# Clean build artifacts
dotnet clean

# Restore packages
dotnet restore
```

## Architecture Overview

This application follows **Clean Architecture** with dependency injection and clear layer separation:

### Layer Dependencies (Inner → Outer)
- **Domain** (core business logic, no dependencies)
- **Application** (interfaces/contracts, depends on Domain)
- **Infrastructure** (external services, depends on Application + Domain)
- **WpfApp** (presentation, depends on all layers)

### Key Architectural Patterns

**Dependency Injection**: Uses Microsoft.Extensions.Hosting with service registration in `App.xaml.cs:ConfigureServices()`. All services are registered here including ViewModels.

**MVVM Pattern**: WPF follows MVVM with `MainWindowViewModel` containing business logic and command handling. The ViewModel is injected via DI.

**Result Pattern**: Operations return `Result<T>` objects instead of throwing exceptions. Check `.IsSuccess` before accessing `.Value`.

**Value Objects**: Domain uses immutable value objects (`DeviceId`, `BlobName`, `LogFilePath`) with validation in constructors.

**Repository/Service Pattern**: Infrastructure services (`IoTHubClient`, `FileUploadService`, `LogWriteService`) implement Application interfaces.

### Azure IoT Hub Integration

The application integrates with Azure IoT Hub for file uploads:
- **Configuration**: IoT Hub connection string and device ID in `appsettings.json`
- **File Upload Flow**: Local file → SAS URI request → Blob upload → Completion notification
- **Retry Policy**: `ExponentialBackoffRetryPolicy` handles transient failures

### Testing Framework

Tests use **xUnit** with:
- **Mocking**: Moq framework for dependencies
- **Assertions**: FluentAssertions for readable test assertions
- **Coverage**: All tests are in `AiDevTest1.Tests` project with comprehensive coverage

### Configuration Management

- **appsettings.json**: Contains IoT Hub connection details
- **Environment-specific**: Supports `appsettings.{Environment}.json` overrides
- **Strongly-typed**: Configuration bound to `IoTHubConfiguration` record

### Domain Models

**LogEntry**: Core entity with timestamp (JST), event type, and device ID
**EventType**: Enum (START, STOP, WARN, ERROR)
**File Format**: JSON Lines (.log files) with daily rotation (yyyy-MM-dd.log)

## Important Development Notes

### Azure Configuration
- IoT Hub connection strings contain sensitive information - never commit real credentials
- Use local development IoT Hub instances for testing
- Device IDs must match between appsettings.json and Azure IoT Hub

### Async/Await Patterns
- UI operations are async to prevent blocking
- File operations and Azure calls use proper async patterns
- ViewModels handle async command execution with progress indication

### Error Handling
- Use Result pattern instead of exceptions for expected failures
- Infrastructure exceptions are caught and converted to Result objects
- UI shows user-friendly error messages via DialogHelper

### Testing Considerations
- Mock all external dependencies (IoT Hub, file system)
- Test both success and failure scenarios
- Use FluentAssertions for readable test assertions
- Async tests require proper async/await patterns