# ══════════════════════════════════════════
# STAGE 1 — BUILD
# Use official .NET 10 SDK image to build
# ══════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

# Set working directory inside container
WORKDIR /src

# Copy project file first (for layer caching)
COPY ["SmartMedicalSystem.csproj", "./"]

# Restore NuGet packages
RUN dotnet restore "SmartMedicalSystem.csproj"

# Copy all remaining source files
COPY . .

# Build the project in Release mode
RUN dotnet build "SmartMedicalSystem.csproj" \
    -c Release \
    -o /app/build

# ══════════════════════════════════════════
# STAGE 2 — PUBLISH
# Create optimized publish output
# ══════════════════════════════════════════
FROM build AS publish

RUN dotnet publish "SmartMedicalSystem.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ══════════════════════════════════════════
# STAGE 3 — FINAL IMAGE
# Use lightweight runtime image (no SDK)
# ══════════════════════════════════════════
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# Set working directory
WORKDIR /app

# Create folder for ML model
RUN mkdir -p /app/MLModels

# Copy published output from stage 2
COPY --from=publish /app/publish .

# Expose port 8080 (HTTP)
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Entry point — run the application
ENTRYPOINT ["dotnet", "SmartMedicalSystem.dll"]