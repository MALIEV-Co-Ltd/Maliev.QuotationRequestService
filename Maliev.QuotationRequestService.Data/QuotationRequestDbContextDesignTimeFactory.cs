using Maliev.QuotationRequestService.Data.DbContexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Maliev.QuotationRequestService.Data;

public class QuotationRequestDbContextDesignTimeFactory : IDesignTimeDbContextFactory<QuotationRequestDbContext>
{
    public QuotationRequestDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<QuotationRequestDbContext>();

        // Use the connection string from environment variable for design time
        var connectionString = Environment.GetEnvironmentVariable("QuotationRequestDbContext")
            ?? "Server=localhost;Port=5432;Database=quotation_request_app_db;User Id=postgres;Password=temp;";

        optionsBuilder.UseNpgsql(connectionString);

        return new QuotationRequestDbContext(optionsBuilder.Options);
    }
}