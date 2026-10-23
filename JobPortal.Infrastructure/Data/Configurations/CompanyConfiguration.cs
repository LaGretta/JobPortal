using JobPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPortal.Infrastructure.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Website).HasMaxLength(200);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.LogoUrl).HasMaxLength(500);
        builder.HasIndex(x => x.OwnerId).IsUnique();

        builder.HasOne(x => x.Owner)
            .WithOne(u => u.Company)
            .HasForeignKey<Company>(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}