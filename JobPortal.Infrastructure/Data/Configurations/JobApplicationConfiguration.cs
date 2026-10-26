using JobPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPortal.Infrastructure.Data.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> builder)
    {
        builder.Property(x => x.CoverLetter).HasMaxLength(2000);

        // ОСЬ ЗАХИСТ ВІД СПАМУ. Пара вакансія кандидат унікальна
        builder.HasIndex(x => new { x.VacancyId, x.CandidateId }).IsUnique();

        builder.HasIndex(x => x.CandidateId);

        builder.HasOne(x => x.Vacancy)
            .WithMany(v => v.Applications)
            .HasForeignKey(x => x.VacancyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Candidate)
            .WithMany(u => u.Applications)
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}