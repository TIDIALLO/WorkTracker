using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkTrack.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "affectations_modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnseignantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SemestreId = table.Column<Guid>(type: "uuid", nullable: false),
                    VolumeHorairePrevu = table.Column<short>(type: "smallint", nullable: true),
                    DateDebut = table.Column<DateOnly>(type: "date", nullable: true),
                    DateFin = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_affectations_modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "annees_academiques",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Libelle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateDebut = table.Column<DateOnly>(type: "date", nullable: false),
                    DateFin = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_annees_academiques", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "apprenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Matricule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DateNaissance = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Statut = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apprenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "enseignants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: false),
                    Specialite = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enseignants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "filieres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nom = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Niveau = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ResponsableId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filieres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "modules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Nom = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Coefficient = table.Column<short>(type: "smallint", nullable: true),
                    Ects = table.Column<short>(type: "smallint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_modules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "presences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeanceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Statut = table.Column<string>(type: "text", nullable: false),
                    MinutesRetard = table.Column<short>(type: "smallint", nullable: true),
                    Justifie = table.Column<bool>(type: "boolean", nullable: false),
                    Commentaire = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MarquePar = table.Column<Guid>(type: "uuid", nullable: false),
                    MarqueLe = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_presences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FiliereId = table.Column<Guid>(type: "uuid", nullable: true),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AnneeScolaire = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: false),
                    EffectifPrevu = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "responsables_pedagogiques",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_responsables_pedagogiques", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "seances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AffectationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Debut = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Fin = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Salle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EstVerrouillee = table.Column<bool>(type: "boolean", nullable: false),
                    CreePar = table.Column<Guid>(type: "uuid", nullable: false),
                    CreeLe = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_seances", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "semestres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnneeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Libelle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Rang = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_semestres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "utilisateurs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Telephone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    MotDePasseHash = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Actif = table.Column<bool>(type: "boolean", nullable: false),
                    CreeLe = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_affectations_modules_ModuleId_PromotionId_EnseignantId_Seme~",
                table: "affectations_modules",
                columns: new[] { "ModuleId", "PromotionId", "EnseignantId", "SemestreId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_apprenants_Matricule",
                table: "apprenants",
                column: "Matricule",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_enseignants_UtilisateurId",
                table: "enseignants",
                column: "UtilisateurId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_modules_Code",
                table: "modules",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_presences_SeanceId_ApprenantId",
                table: "presences",
                columns: new[] { "SeanceId", "ApprenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_promotions_Nom_AnneeScolaire",
                table: "promotions",
                columns: new[] { "Nom", "AnneeScolaire" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_seances_AffectationId_Debut",
                table: "seances",
                columns: new[] { "AffectationId", "Debut" });

            migrationBuilder.CreateIndex(
                name: "IX_semestres_AnneeId_Rang",
                table: "semestres",
                columns: new[] { "AnneeId", "Rang" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_utilisateurs_Email",
                table: "utilisateurs",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "affectations_modules");

            migrationBuilder.DropTable(
                name: "annees_academiques");

            migrationBuilder.DropTable(
                name: "apprenants");

            migrationBuilder.DropTable(
                name: "enseignants");

            migrationBuilder.DropTable(
                name: "filieres");

            migrationBuilder.DropTable(
                name: "modules");

            migrationBuilder.DropTable(
                name: "presences");

            migrationBuilder.DropTable(
                name: "promotions");

            migrationBuilder.DropTable(
                name: "responsables_pedagogiques");

            migrationBuilder.DropTable(
                name: "seances");

            migrationBuilder.DropTable(
                name: "semestres");

            migrationBuilder.DropTable(
                name: "utilisateurs");
        }
    }
}
