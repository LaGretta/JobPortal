using JobPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPortal.Infrastructure.Data.Configurations;

public class VacancySkillConfiguration : IEntityTypeConfiguration<VacancySkill>
{
    public void Configure(EntityTypeBuilder<VacancySkill> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);

        builder.HasIndex(x => x.Name);                                   // під фільтр по навичці
        builder.HasIndex(x => new { x.VacancyId, x.Name }).IsUnique();   // одна навичка раз на вакансію

        builder.HasOne(x => x.Vacancy)
            .WithMany(v => v.Skills)
            .HasForeignKey(x => x.VacancyId)
            .OnDelete(DeleteBehavior.Cascade);   // вакансію видалили навички з нею
    }
}