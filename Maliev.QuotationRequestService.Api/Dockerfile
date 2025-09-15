# Use the official .NET 9.0 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the official .NET 9.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["Maliev.QuotationRequestService.Api.csproj", "Maliev.QuotationRequestService.Api/"]
COPY ["../Maliev.QuotationRequestService.Data/Maliev.QuotationRequestService.Data.csproj", "Maliev.QuotationRequestService.Data/"]

# Restore dependencies
RUN dotnet restore "Maliev.QuotationRequestService.Api/Maliev.QuotationRequestService.Api.csproj"

# Copy all source files
COPY . Maliev.QuotationRequestService.Api/
COPY ../Maliev.QuotationRequestService.Data/ Maliev.QuotationRequestService.Data/

# Build the application
WORKDIR "/src/Maliev.QuotationRequestService.Api"
RUN dotnet build "Maliev.QuotationRequestService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Maliev.QuotationRequestService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image
FROM base AS final
WORKDIR /app

# Copy the published application
COPY --from=publish /app/publish .

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

# Create non-root user for security
RUN addgroup --system --gid 1001 dotnet && \
    adduser --system --uid 1001 --gid 1001 --shell /bin/false --home /app dotnet

# Change ownership of app directory
RUN chown -R dotnet:dotnet /app
USER dotnet

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/quotation-requests/liveness || exit 1

ENTRYPOINT ["dotnet", "Maliev.QuotationRequestService.Api.dll"]