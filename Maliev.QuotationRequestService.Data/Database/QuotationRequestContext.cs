using Maliev.QuotationRequestService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Maliev.QuotationRequestService.Data.Database
{
    public class QuotationRequestContext : DbContext
    {
        public QuotationRequestContext(DbContextOptions<QuotationRequestContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Request> Requests { get; set; }
        public virtual DbSet<RequestFile> RequestFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Request>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Country).HasMaxLength(256);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.TaxIdentification).HasMaxLength(256);

                entity.Property(e => e.TelephoneNumber).HasMaxLength(256);

                entity.HasMany(r => r.RequestFiles)
                      .WithOne(rf => rf.Request)
                      .HasForeignKey(rf => rf.RequestId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RequestFile>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Bucket).HasMaxLength(50);

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.ModifiedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getutcdate())");

                entity.Property(e => e.RequestId).HasColumnName("RequestID");
            });
        }
    }
}