using Maliev.QuotationRequestService.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Maliev.QuotationRequestService.Data.DbContexts;

public class QuotationRequestDbContext : DbContext
{
    public QuotationRequestDbContext(DbContextOptions<QuotationRequestDbContext> options) : base(options)
    {
    }

    public DbSet<QuotationRequest> QuotationRequests { get; set; }
    public DbSet<QuotationRequestFile> QuotationRequestFiles { get; set; }
    public DbSet<QuotationRequestComment> QuotationRequestComments { get; set; }
    public DbSet<QuotationRequestStatusHistory> QuotationRequestStatusHistories { get; set; }

    public override int SaveChanges()
    {
        AddTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AddTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void AddTimestamps()
    {
        var entities = ChangeTracker.Entries()
            .Where(x => x.Entity is IAuditable && (x.State == EntityState.Added || x.State == EntityState.Modified));

        foreach (var entity in entities)
        {
            var auditableEntity = (IAuditable)entity.Entity;
            var now = DateTimeOffset.UtcNow;

            if (entity.State == EntityState.Added)
            {
                // Only set CreatedAt if it hasn't been explicitly set (is default DateTimeOffset)
                if (auditableEntity.CreatedAt == DateTimeOffset.MinValue)
                {
                    auditableEntity.CreatedAt = now;
                }
            }

            // Only set UpdatedAt if it hasn't been explicitly set (is default DateTimeOffset)
            if (auditableEntity.UpdatedAt == DateTimeOffset.MinValue)
            {
                auditableEntity.UpdatedAt = now;
            }
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure QuotationRequest entity
        modelBuilder.Entity<QuotationRequest>(entity =>
        {
            entity.ToTable("QuotationRequests");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.RequestNumber)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(e => e.RequestNumber)
                .IsUnique()
                .HasDatabaseName("IX_QuotationRequests_RequestNumber");

            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.CustomerEmail)
                .IsRequired()
                .HasMaxLength(254);

            entity.Property(e => e.CustomerPhone)
                .HasMaxLength(20);

            entity.Property(e => e.CompanyName)
                .HasMaxLength(200);

            entity.Property(e => e.JobTitle)
                .HasMaxLength(100);

            entity.Property(e => e.Subject)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Description)
                .IsRequired();

            entity.Property(e => e.Requirements)
                .HasMaxLength(500);

            entity.Property(e => e.Industry)
                .HasMaxLength(50);

            entity.Property(e => e.ProjectTimeline)
                .HasMaxLength(100);

            entity.Property(e => e.EstimatedBudget)
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.PreferredContactMethod)
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.Priority)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.AssignedToTeamMember)
                .HasMaxLength(100);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Indexes for performance
            entity.HasIndex(e => e.CustomerEmail);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.AssignedToTeamMember);
            entity.HasIndex(e => e.CustomerId);
        });

        // Configure QuotationRequestFile entity
        modelBuilder.Entity<QuotationRequestFile>(entity =>
        {
            entity.ToTable("QuotationRequestFiles");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.ObjectName)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.ContentType)
                .HasMaxLength(100);

            entity.Property(e => e.UploadServiceFileId)
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.FileCategory)
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Foreign key relationship
            entity.HasOne(qrf => qrf.QuotationRequest)
                .WithMany(qr => qr.Files)
                .HasForeignKey(qrf => qrf.QuotationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for foreign key
            entity.HasIndex(e => e.QuotationRequestId);
        });

        // Configure QuotationRequestComment entity
        modelBuilder.Entity<QuotationRequestComment>(entity =>
        {
            entity.ToTable("QuotationRequestComments");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.AuthorName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.AuthorEmail)
                .HasMaxLength(254);

            entity.Property(e => e.Content)
                .IsRequired();

            entity.Property(e => e.CommentType)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Foreign key relationship
            entity.HasOne(qrc => qrc.QuotationRequest)
                .WithMany(qr => qr.Comments)
                .HasForeignKey(qrc => qrc.QuotationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for foreign key
            entity.HasIndex(e => e.QuotationRequestId);
            entity.HasIndex(e => e.CommentType);
        });

        // Configure QuotationRequestStatusHistory entity
        modelBuilder.Entity<QuotationRequestStatusHistory>(entity =>
        {
            entity.ToTable("QuotationRequestStatusHistories");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            entity.Property(e => e.FromStatus)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.ToStatus)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.ChangedByTeamMember)
                .HasMaxLength(100);

            entity.Property(e => e.ChangeReason)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Foreign key relationship
            entity.HasOne(qrsh => qrsh.QuotationRequest)
                .WithMany(qr => qr.StatusHistory)
                .HasForeignKey(qrsh => qrsh.QuotationRequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for foreign key and performance
            entity.HasIndex(e => e.QuotationRequestId);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ToStatus);
        });
    }
}