using ASC.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ASC.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<MasterDataKey> MasterDataKeys { get; set; }
        public DbSet<MasterDataValue> MasterDataValues { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<MasterDataValue>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            builder.Entity<ServiceRequest>()
                .Property(p => p.EstimatedPrice)
                .HasColumnType("decimal(18,2)");

            builder.Entity<ServiceRequest>()
                .Property(p => p.FinalPrice)
                .HasColumnType("decimal(18,2)");

            // Index để query chat nhanh hơn
            builder.Entity<ChatMessage>()
                .HasIndex(c => c.ServiceRequestId);
        }
    }
}
