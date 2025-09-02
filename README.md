# Maliev.QuotationRequestService

This repository contains the `Maliev.QuotationRequestService` project, migrated to a modern .NET 9 solution.

## Getting Started

### Prerequisites

*   .NET 9 SDK (or later)
*   SQL Server (for local development, SQL Server LocalDB is sufficient)

### Local Development Setup

1.  **Clone the repository**:
    ```bash
    git clone <repository_url>
    cd Maliev.QuotationRequestService
    ```

2.  **Restore NuGet packages**:
    ```bash
    dotnet restore
    ```

3.  **Configure User Secrets**:
    Sensitive information like JWT keys and database connection strings are managed using .NET User Secrets for local development. You need to set these secrets. Replace the placeholder values with your actual secrets:

    ```bash
    dotnet user-secrets set "Jwt:Issuer" "your_jwt_issuer" --project Maliev.QuotationRequestService.Api\Maliev.QuotationRequestService.Api.csproj
    dotnet user-secrets set "Jwt:Audience" "your_jwt_audience" --project Maliev.QuotationRequestService.Api\Maliev.QuotationRequestService.Api.csproj
    dotnet user-secrets set "JwtSecurityKey" "your_jwt_security_key_very_long_and_secure" --project Maliev.QuotationRequestService.Api\Maliev.QuotationRequestService.Api.csproj
    dotnet user-secrets set "ConnectionStrings:QuotationRequestServiceDbContext" "Server=(localdb)\MSSQLLocalDB;Database=QuotationRequestServiceDb;Trusted_Connection=True;MultipleActiveResultSets=true" --project Maliev.QuotationRequestService.Api\Maliev.QuotationRequestService.Api.csproj
    ```

4.  **Run Database Migrations (Optional, if you have migrations)**:
    If you have Entity Framework Core migrations, you can apply them to your local database:
    ```bash
    dotnet ef database update --project Maliev.QuotationRequestService.Data
    ```

5.  **Run the Application**:
    ```bash
    dotnet run --project Maliev.QuotationRequestService.Api
    ```
    The API will typically run on `https://localhost:7000` (or a similar port). The Swagger UI will be available at `https://localhost:7000/quotationrequests/swagger`.

### Running Tests

To run all unit tests:

```bash
dotnet test
```

## Deployment

For deployment to production environments, secrets should be managed securely using solutions like Google Secret Manager. Refer to the `GEMINI.md` file for more details on secret management and Kubernetes configuration.
