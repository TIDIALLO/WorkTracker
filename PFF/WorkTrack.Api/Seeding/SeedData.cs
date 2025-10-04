// WorkTrack.Api/Seeding/SeedData.cs
using Microsoft.EntityFrameworkCore;
using WorkTrack.Infrastructure;
using WorkTrack.Domain.Entities;

namespace WorkTrack.Api.Seeding;

public static class SeedData
{
    /// <summary>Seed *idempotent* pour l'environnement dev.</summary>
    public static async Task SeedDevAsync(AppDbContext db)
    {
        // déjà seedé ?
        if (await db.Modules.AnyAsync()) return;

        // Utilisateurs
        var uProf = new Utilisateur { Id = Guid.NewGuid(), Prenom = "Aïda", Nom = "Fall", Email = "aida.fall@ecole.test", MotDePasseHash = "dev", Role = RoleUtilisateur.Enseignant };
        var uResp = new Utilisateur { Id = Guid.NewGuid(), Prenom = "Moussa", Nom = "Diop", Email = "moussa.diop@ecole.test", MotDePasseHash = "dev", Role = RoleUtilisateur.Responsable };
        var uSt1  = new Utilisateur { Id = Guid.NewGuid(), Prenom = "Fatou", Nom = "Ndiaye", Email = "fatou.ndiaye@etu.test", MotDePasseHash = "dev", Role = RoleUtilisateur.Apprenant };
        var uSt2  = new Utilisateur { Id = Guid.NewGuid(), Prenom = "Ibrahima", Nom = "Ba", Email = "ibrahima.ba@etu.test", MotDePasseHash = "dev", Role = RoleUtilisateur.Apprenant };
        var uSt3  = new Utilisateur { Id = Guid.NewGuid(), Prenom = "Khadija", Nom = "Sow", Email = "khadija.sow@etu.test", MotDePasseHash = "dev", Role = RoleUtilisateur.Apprenant };
        db.Utilisateurs.AddRange(uProf, uResp, uSt1, uSt2, uSt3);

        // Rôles annexes
        var resp = new ResponsablePedagogique { Id = Guid.NewGuid(), UtilisateurId = uResp.Id };
        var ens  = new Enseignant { Id = Guid.NewGuid(), UtilisateurId = uProf.Id, Specialite = "Math" };
        db.Responsables.Add(resp); db.Enseignants.Add(ens);

        // Filière / Promo
        var fil = new Filiere { Id = Guid.NewGuid(), Nom = "Informatique", Niveau = "L2", ResponsableId = resp.Id };
        var promo = new Promotion { Id = Guid.NewGuid(), FiliereId = fil.Id, Nom = "L2-Info-A", AnneeScolaire = "2024-2025", EffectifPrevu = 40 };
        db.Filieres.Add(fil); db.Promotions.Add(promo);

        // Apprenants
        var st1 = new Apprenant { Id = Guid.NewGuid(), UtilisateurId = uSt1.Id, PromotionId = promo.Id, Matricule = "ETU-2024-0001" };
        var st2 = new Apprenant { Id = Guid.NewGuid(), UtilisateurId = uSt2.Id, PromotionId = promo.Id, Matricule = "ETU-2024-0002" };
        var st3 = new Apprenant { Id = Guid.NewGuid(), UtilisateurId = uSt3.Id, PromotionId = promo.Id, Matricule = "ETU-2024-0003" };
        db.Apprenants.AddRange(st1, st2, st3);

        // Calendrier
        var an  = new AnneeAcademique { Id = Guid.NewGuid(), Libelle = "2024-2025", DateDebut = new DateOnly(2024,10,1), DateFin = new DateOnly(2025,7,31) };
        var s1  = new Semestre { Id = Guid.NewGuid(), AnneeId = an.Id, Libelle = "S1", Rang = 1 };
        db.Annees.Add(an); db.Semestres.Add(s1);

        // Modules
        var mod1 = new Module { Id = Guid.NewGuid(), Code = "ALG2", Nom = "Algorithmes II", Coefficient = 2, Ects = 4 };
        var mod2 = new Module { Id = Guid.NewGuid(), Code = "BD1",  Nom = "Bases de données I", Coefficient = 2, Ects = 4 };
        db.Modules.AddRange(mod1, mod2);

        // Affectations
        var aff1 = new AffectationModule { Id = Guid.NewGuid(), ModuleId = mod1.Id, PromotionId = promo.Id, EnseignantId = ens.Id, SemestreId = s1.Id, VolumeHorairePrevu = 30 };
        var aff2 = new AffectationModule { Id = Guid.NewGuid(), ModuleId = mod2.Id, PromotionId = promo.Id, EnseignantId = ens.Id, SemestreId = s1.Id, VolumeHorairePrevu = 30 };
        db.Affectations.AddRange(aff1, aff2);

        // Séances (5 jours × 2)
        var baseDay = DateTimeOffset.UtcNow.Date.AddDays(-3);
        var seances = new List<Seance>();
        for (int d = 0; d < 5; d++)
        {
            seances.Add(new Seance { Id = Guid.NewGuid(), AffectationId = aff1.Id, Debut = baseDay.AddDays(d).AddHours(8),  Fin = baseDay.AddDays(d).AddHours(10), Salle = "A-101", EstVerrouillee = false, CreePar = uProf.Id });
            seances.Add(new Seance { Id = Guid.NewGuid(), AffectationId = aff2.Id, Debut = baseDay.AddDays(d).AddHours(11), Fin = baseDay.AddDays(d).AddHours(13), Salle = "B-202", EstVerrouillee = false, CreePar = uProf.Id });
        }
        db.Seances.AddRange(seances);

        await db.SaveChangesAsync();

        // Présences aléatoires
        var rnd = new Random();
        foreach (var s in seances)
        {
            foreach (var st in new[] { st1, st2, st3 })
            {
                var draw = rnd.Next(100);
                db.Presences.Add(new Presence
                {
                    Id = Guid.NewGuid(),
                    SeanceId = s.Id,
                    ApprenantId = st.Id,
                    Statut = draw < 80 ? StatutPresence.Present : (draw < 90 ? StatutPresence.Retard : StatutPresence.Absent),
                    MinutesRetard = (draw >= 80 && draw < 90 ? (short?)rnd.Next(1, 15) : null),
                    Justifie = false,
                    MarquePar = uProf.Id,
                    MarqueLe = s.Debut.AddMinutes(10)
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
