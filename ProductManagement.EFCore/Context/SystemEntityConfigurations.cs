using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductManagement.Domain.Entities.SystemManagement;

namespace ProductManagement.EfCore.Context
{
    public class SystemSettingConfiguration : IEntityTypeConfiguration<SystemSetting>
    {
        public void Configure(EntityTypeBuilder<SystemSetting> builder)
        {
            builder.HasIndex(s => new { s.Category, s.Key })
                .IsUnique()
                .HasDatabaseName("IX_SystemSettings_Category_Key");

            builder.Property(s => s.Category)
                .HasMaxLength(100);

            builder.Property(s => s.Key)
                .HasMaxLength(150);

            builder.Property(s => s.DataType)
                .HasMaxLength(20);

            builder.Property(s => s.DisplayName)
                .HasMaxLength(200);

            builder.Property(s => s.Description)
                .HasMaxLength(500);

            builder.Property(s => s.UpdatedBy)
                .HasMaxLength(256);
        }
    }

    public class IntegrationConfiguration : IEntityTypeConfiguration<Integration>
    {
        public void Configure(EntityTypeBuilder<Integration> builder)
        {
            builder.HasIndex(i => i.ProviderKey)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0")
                .HasDatabaseName("IX_Integrations_ProviderKey");

            builder.Property(i => i.Name)
                .HasMaxLength(150);

            builder.Property(i => i.Type)
                .HasMaxLength(50);

            builder.Property(i => i.ProviderKey)
                .HasMaxLength(100);

            builder.Property(i => i.Description)
                .HasMaxLength(500);

            builder.Property(i => i.LastTestMessage)
                .HasMaxLength(500);
        }
    }
}
