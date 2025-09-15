# Maliev Quotation Request Service

A comprehensive microservice for handling customer quotation requests for Maliev Co. Ltd.'s 3D printing and manufacturing business. This service provides RESTful APIs for submitting, managing, and tracking quotation requests with complete audit trails and file attachment support.

## 🚀 Service Overview

The Quotation Request Service manages the entire lifecycle of customer quotation requests, from initial submission through final completion. It supports both authenticated customers and guest submissions, ensuring no potential business opportunity is missed.

### Key Features

- **Customer Request Management**: Submit and track quotation requests
- **File Attachment Support**: Integration with UploadService for 3D models and design files
- **Status Tracking**: Complete audit trail with status history
- **Team Collaboration**: Internal comments system for team coordination
- **Rate Limiting**: Protection against abuse with configurable limits
- **Caching**: High-performance caching for frequently accessed data
- **Monitoring**: Prometheus metrics and health checks

## 📋 Business Requirements

- Handle quotation requests from website customers
- Support both signed-in and guest customers
- Focus on traceability, efficiency, and transparency
- Ensure no requests are left untouched
- Comprehensive audit logging and status tracking

## 🏗️ Architecture

### Technology Stack

- **Framework**: ASP.NET Core 9.0
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Monitoring**: Prometheus metrics
- **Logging**: Serilog with structured logging
- **Caching**: In-memory caching
- **Testing**: xUnit, FluentAssertions, Moq

### Project Structure

```
Maliev.QuotationRequestService/
├── .github/workflows/          # CI/CD pipelines (develop, staging, main)
├── Maliev.QuotationRequestService.Api/     # Web API project
├── Maliev.QuotationRequestService.Data/    # Data layer
├── Maliev.QuotationRequestService.Tests/   # Unit and integration tests
├── Dockerfile                  # Container configuration
├── README.md                   # This file
└── Maliev.QuotationRequestService.sln      # Solution file
```

## 🔌 API Endpoints

### Public Endpoints (No Authentication Required)

#### Submit Quotation Request
```http
POST /api/v1/quotation-requests
Content-Type: application/json

{
  "customerName": "John Doe",
  "customerEmail": "john@example.com",
  "customerPhone": "+1234567890",
  "companyName": "Example Corp",
  "subject": "3D Printing Project",
  "description": "Need custom parts printed",
  "requirements": "High precision required",
  "industry": "Automotive",
  "projectTimeline": "2 weeks",
  "estimatedBudget": 5000.00,
  "preferredContactMethod": "Email",
  "fileIds": ["uuid1", "uuid2"]
}
```

#### Get Request by Request Number
```http
GET /api/v1/quotation-requests/by-request-number/{requestNumber}
```

### Admin Endpoints (Authentication Required)

#### Get All Requests by Status
```http
GET /api/v1/quotation-requests/by-status/{status}
Authorization: Bearer {jwt_token}
```

#### Update Request Status
```http
PUT /api/v1/quotation-requests/{id}/status
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "newStatus": "InReview",
  "updatedBy": "admin@maliev.com",
  "notes": "Moving to review phase"
}
```

#### Add Comment
```http
POST /api/v1/quotation-requests/{id}/comments
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "content": "Customer requirements clarified",
  "authorName": "admin@maliev.com"
}
```

## 🗄️ Database Schema

### Core Entities

- **QuotationRequest**: Main entity storing customer information and request details
- **QuotationRequestFile**: File attachments linked to requests
- **QuotationRequestComment**: Internal team comments
- **QuotationRequestStatusHistory**: Complete audit trail of status changes

### Status Flow

```
New → InReview → AdditionalInfoRequired → UnderEvaluation → QuotationPreparing → Quoted → Accepted/Rejected/Cancelled
```

### Priority Levels

- Low, Medium, High, Urgent

## 🚀 Development Setup

### Prerequisites

- .NET 9.0 SDK
- PostgreSQL
- Docker (optional)
- kubectl (for Kubernetes deployment)

### Local Development

1. **Clone the repository**
   ```bash
   git clone https://github.com/MALIEV-Co-Ltd/Maliev.QuotationRequestService.git
   cd Maliev.QuotationRequestService
   ```

2. **Set up database connection**
   ```bash
   # For local PostgreSQL
   export QuotationRequestDbContext="Server=localhost;Port=5432;Database=quotation_request_app_db;User Id=postgres;Password=your_password;"

   # For development cluster (with port-forward)
   kubectl port-forward -n maliev-dev svc/postgres-cluster-rw 5432:5432
   export QuotationRequestDbContext="Server=localhost;Port=5432;Database=quotation_request_app_db;User Id=postgres;Password=ACTUAL_PASSWORD;"
   ```

3. **Run database migrations**
   ```bash
   dotnet ef database update --project Maliev.QuotationRequestService.Data --startup-project Maliev.QuotationRequestService.Api
   ```

4. **Build and run**
   ```bash
   dotnet restore Maliev.QuotationRequestService.sln
   dotnet build Maliev.QuotationRequestService.sln
   dotnet run --project Maliev.QuotationRequestService.Api
   ```

5. **Run tests**
   ```bash
   dotnet test Maliev.QuotationRequestService.sln --verbosity normal
   ```

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `QuotationRequestDbContext` | PostgreSQL connection string | Required |
| `UploadService__BaseUrl` | Upload service endpoint | `http://localhost:8080` |
| `UploadService__TimeoutSeconds` | Upload service timeout | `30` |

## 🐳 Docker

### Build Image
```bash
docker build -t maliev-quotation-request-service .
```

### Run Container
```bash
docker run -p 8080:8080 \
  -e QuotationRequestDbContext="Server=host.docker.internal;Port=5432;Database=quotation_request_app_db;User Id=postgres;Password=your_password;" \
  maliev-quotation-request-service
```

## ☸️ Kubernetes Deployment

This service uses GitOps deployment with ArgoCD. Deployment manifests are maintained in the separate `maliev-gitops` repository.

### CI/CD Pipeline

1. **Code Push**: Push to develop/staging/main branch
2. **GitHub Actions**: Build, test, and push Docker image
3. **GitOps Update**: Update image reference in maliev-gitops repo
4. **ArgoCD Sync**: Automatically deploy to Kubernetes cluster

### Monitoring Endpoints

- **Liveness**: `/quotation-requests/liveness`
- **Readiness**: `/quotation-requests/readiness`
- **Metrics**: `/quotation-requests/metrics` (Prometheus format)
- **Swagger**: `/quotation-requests/swagger` (non-production)

## 📊 Rate Limiting

- **Global Policy**: 1000 requests per minute per IP
- **Quotation Request Policy**: 5 submissions per minute per IP (public endpoint)

## 🔧 Configuration

### Secrets Management

Secrets are managed via Google Secret Manager and mounted at `/mnt/secrets` in production:

- Database connection strings
- JWT signing keys
- External service credentials

### Development Fallbacks

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.Configure<UploadServiceOptions>(options =>
    {
        options.BaseUrl = "http://localhost:8080";
        options.TimeoutSeconds = 30;
    });
}
```

## 🧪 Testing

### Test Categories

- **Unit Tests**: Service layer, data models, business logic
- **Integration Tests**: API endpoints, database operations
- **Health Check Tests**: Monitoring endpoints

### Test Commands

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test class
dotnet test --filter "ClassName=QuotationRequestServiceTests"
```

## 📈 Monitoring

### Prometheus Metrics

The service exposes standard ASP.NET Core metrics plus custom business metrics:

- HTTP request duration and count
- Database operation metrics
- Cache hit/miss ratios
- Business operation counters

### Health Checks

- **Database connectivity**
- **Memory usage**
- **Dependency service availability**

### Grafana Dashboards

Access monitoring dashboards:
```powershell
cd maliev-gitops
.\scripts\open-grafana.ps1
```

## 🔐 Security

- **Authentication**: JWT Bearer tokens for admin endpoints
- **Authorization**: Role-based access control
- **Rate Limiting**: Protection against abuse
- **Input Validation**: Comprehensive data validation
- **SQL Injection Protection**: Entity Framework parameterized queries
- **CORS**: Configured for production domains

## 🤝 Integration

### UploadService Integration

The service integrates with the UploadService for file management:

```csharp
// Get file metadata
var metadata = await _uploadServiceClient.GetFileMetadataAsync(fileId);

// Associate files with quotation request
foreach (var fileId in request.FileIds)
{
    var fileMetadata = await _uploadServiceClient.GetFileMetadataAsync(fileId);
    if (fileMetadata != null)
    {
        quotationRequest.Files.Add(new QuotationRequestFile
        {
            FileId = fileId,
            FileName = fileMetadata.FileName,
            FileSize = fileMetadata.FileSize,
            ContentType = fileMetadata.ContentType
        });
    }
}
```

## 🐛 Troubleshooting

### Common Issues

1. **Database Connection Failed**
   - Verify PostgreSQL is running
   - Check connection string format
   - Ensure database exists

2. **Rate Limiting Triggered**
   - Check IP-based rate limits
   - Review request patterns
   - Consider request throttling

3. **File Upload Integration Issues**
   - Verify UploadService availability
   - Check file ID validity
   - Review timeout settings

### Debug Commands

```bash
# Check service logs
kubectl logs -f deployment/maliev-quotation-request-service -n maliev-dev

# Port forward for local testing
kubectl port-forward -n maliev-dev svc/maliev-quotation-request-service 8080:8080

# Check database connectivity
dotnet ef database update --dry-run
```

## 📚 Additional Resources

- [CLAUDE.md](./CLAUDE.md) - Development guidelines and patterns
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Prometheus Metrics](https://prometheus.io/docs/concepts/metric_types/)

## 🤝 Contributing

1. Follow patterns defined in `CLAUDE.md`
2. Ensure all tests pass before submitting PR
3. Update documentation for new features
4. Follow semantic versioning for releases

## 📄 License

Copyright © 2025 Maliev Co. Ltd. All rights reserved.
