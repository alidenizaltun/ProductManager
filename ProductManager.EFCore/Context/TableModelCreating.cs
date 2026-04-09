using ProductManager.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ProductManager.EfCore.Context
{
    public static class TableModelCreating
    {
        public static void ConfigureIdentityTables(this ModelBuilder mb)
        {
            const string schema = "Identity";

            mb.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("Users", schema);

                entity.Property(u => u.FirstName)
                    .HasMaxLength(100);

                entity.Property(u => u.LastName)
                    .HasMaxLength(100);

                entity.Property(u => u.RefreshToken)
                    .HasMaxLength(500);

                entity.HasIndex(u => u.Email)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");
            });

            mb.Entity<ApplicationRole>(entity =>
            {
                entity.ToTable("Roles", schema);

                entity.Property(r => r.Description)
                    .HasMaxLength(500);
            });

            mb.Entity<IdentityUserRole<Guid>>(entity =>
            {
                entity.ToTable("UserRoles", schema);
            });

            mb.Entity<IdentityUserClaim<Guid>>(entity =>
            {
                entity.ToTable("UserClaims", schema);
            });

            mb.Entity<IdentityUserLogin<Guid>>(entity =>
            {
                entity.ToTable("UserLogins", schema);
            });

            mb.Entity<IdentityRoleClaim<Guid>>(entity =>
            {
                entity.ToTable("RoleClaims", schema);
            });

            mb.Entity<IdentityUserToken<Guid>>(entity =>
            {
                entity.ToTable("UserTokens", schema);
            });
        }

        public static void ConfigureTable(this ModelBuilder mb) { }
    }
}
