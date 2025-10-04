using Microsoft.EntityFrameworkCore;
using WorkTrack.Domain.Entities;

namespace WorkTrack.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<ResponsablePedagogique> Responsables => Set<ResponsablePedagogique>();
    public DbSet<Enseignant> Enseignants => Set<Enseignant>();
    public DbSet<Filiere> Filieres => Set<Filiere>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<Apprenant> Apprenants => Set<Apprenant>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<AnneeAcademique> Annees => Set<AnneeAcademique>();
    public DbSet<Semestre> Semestres => Set<Semestre>();
    public DbSet<AffectationModule> Affectations => Set<AffectationModule>();
    public DbSet<Seance> Seances => Set<Seance>();
    public DbSet<Presence> Presences => Set<Presence>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<Utilisateur>(e => {
            e.ToTable("utilisateurs");
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>();
        });

        b.Entity<ResponsablePedagogique>().ToTable("responsables_pedagogiques");

        b.Entity<Enseignant>(e => {
            e.ToTable("enseignants");
            e.HasIndex(x => x.UtilisateurId).IsUnique();
        });

        b.Entity<Filiere>().ToTable("filieres");

        b.Entity<Promotion>(e => {
            e.ToTable("promotions");
            e.HasIndex(x => new { x.Nom, x.AnneeScolaire }).IsUnique();
        });

        b.Entity<Apprenant>(e => {
            e.ToTable("apprenants");
            e.HasIndex(x => x.Matricule).IsUnique();
        });

        b.Entity<Module>(e => {
            e.ToTable("modules");
            e.HasIndex(x => x.Code).IsUnique();
        });

        b.Entity<AnneeAcademique>().ToTable("annees_academiques");

        b.Entity<Semestre>(e => {
            e.ToTable("semestres");
            e.HasIndex(x => new { x.AnneeId, x.Rang }).IsUnique();
        });

        b.Entity<AffectationModule>(e => {
            e.ToTable("affectations_modules");
            e.HasIndex(x => new { x.ModuleId, x.PromotionId, x.EnseignantId, x.SemestreId }).IsUnique();
        });

        b.Entity<Seance>(e => {
            e.ToTable("seances");
            e.HasIndex(x => new { x.AffectationId, x.Debut });
            e.Property(x => x.CreeLe).HasDefaultValueSql("now()");
        });

        b.Entity<Presence>(e => {
            e.ToTable("presences");
            e.HasIndex(x => new { x.SeanceId, x.ApprenantId }).IsUnique();
            e.Property(x => x.Statut).HasConversion<string>();
            e.Property(x => x.MarqueLe).HasDefaultValueSql("now()");
        });
    }
}
