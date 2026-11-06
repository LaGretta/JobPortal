using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobPortal.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilesFavoritesAndVacancyFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Vacancies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceLevel",
                table: "Vacancies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CvSnapshotBlobPath",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvSnapshotContentType",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CvSnapshotFileName",
                table: "JobApplications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CandidateProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Headline = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    About = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CvFileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    CvBlobPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CvContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CvSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    CvUploadedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidateProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidateProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FavoriteVacancies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VacancyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteVacancies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FavoriteVacancies_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteVacancies_Vacancies_VacancyId",
                        column: x => x.VacancyId,
                        principalTable: "Vacancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VacancySkills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VacancyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VacancySkills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VacancySkills_Vacancies_VacancyId",
                        column: x => x.VacancyId,
                        principalTable: "Vacancies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidateProfiles_UserId",
                table: "CandidateProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteVacancies_UserId_VacancyId",
                table: "FavoriteVacancies",
                columns: new[] { "UserId", "VacancyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteVacancies_VacancyId",
                table: "FavoriteVacancies",
                column: "VacancyId");

            migrationBuilder.CreateIndex(
                name: "IX_VacancySkills_Name",
                table: "VacancySkills",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_VacancySkills_VacancyId_Name",
                table: "VacancySkills",
                columns: new[] { "VacancyId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CandidateProfiles");

            migrationBuilder.DropTable(
                name: "FavoriteVacancies");

            migrationBuilder.DropTable(
                name: "VacancySkills");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "ExperienceLevel",
                table: "Vacancies");

            migrationBuilder.DropColumn(
                name: "CvSnapshotBlobPath",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CvSnapshotContentType",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "CvSnapshotFileName",
                table: "JobApplications");
        }
    }
}
