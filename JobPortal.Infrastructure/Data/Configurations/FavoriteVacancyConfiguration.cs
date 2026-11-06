using JobPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobPortal.Infrastructure.Data.Configurations;

public class FavoriteVacancyConfiguration : IEntityTypeConfiguration<FavoriteVacancy>
{
    public void Configure(EntityTypeBuilder<FavoriteVacancy> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.UserId, x.VacancyId }).IsUnique();  // двічі в обране не вийде

        builder.HasOne(x => x.User)
            .WithMany(u => u.Favorites)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Vacancy)
            .WithMany(v => v.Favorites)
            .HasForeignKey(x => x.VacancyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}