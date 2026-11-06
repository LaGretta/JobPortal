using JobPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPortal.Infrastructure.Data.Configurations;

public class CandidateProfileConfiguration : IEntityTypeConfiguration<CandidateProfile>
{
    public void Configure(EntityTypeBuilder<CandidateProfile> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Headline).HasMaxLength(200);
        builder.Property(x => x.About).HasMaxLength(2000);
        builder.Property(x => x.CvFileName).HasMaxLength(260);   // ліміт назви файлу у Windows
        builder.Property(x => x.CvBlobPath).HasMaxLength(500);
        builder.Property(x => x.CvContentType).HasMaxLength(100);

        builder.HasIndex(x => x.UserId).IsUnique();              

        builder.HasOne(x => x.User)
            .WithOne(u => u.CandidateProfile)                   
            .HasForeignKey<CandidateProfile>(x => x.UserId)      
            .OnDelete(DeleteBehavior.Cascade);                  
    }
}