// WorkTrack.Client/Models/Dtos.cs
namespace WorkTrack.Client.Models;

public enum AttendanceStatusDto { Present, Absent, Retard }

public record SeanceDto(
    Guid Id, DateTimeOffset Debut, DateTimeOffset Fin, string? Salle,
    string ModuleCode, string ModuleNom, string PromotionNom);

public record RosterStudentDto(Guid ApprenantId, string Matricule, string NomComplet);

public record AttendanceMarkDto(Guid ApprenantId, AttendanceStatusDto Statut, int? MinutesRetard, string? Commentaire);
public record AttendanceRowDto(
    Guid ApprenantId, string Matricule, string NomComplet,
    string Statut, int? MinutesRetard, string? Commentaire);

public class EnseignantDto
    {
        public Guid Id { get; set; }
        public Guid UtilisateurId { get; set; }
        public string? Prenom { get; set; }
        public string? Nom { get; set; }
        public string? Email { get; set; }
        public string? Specialite { get; set; }
    }

    public class EnseignantCreateOrUpdateDto
    {
        public Guid? UtilisateurId { get; set; }
        public string? Specialite { get; set; }
    }

    public class UtilisateurDto
    {
        public Guid Id { get; set; }
        public string? Prenom { get; set; }
        public string? Nom { get; set; }
        public string? Email { get; set; }
    }

    public class ModuleDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public short? Coefficient { get; set; }
        public short? Ects { get; set; }
    }
    
    public class AffectationModuleDto
    {
        public Guid Id { get; set; }
        public Guid ModuleId { get; set; }
        public Guid PromotionId { get; set; }
        public Guid EnseignantId { get; set; }
        public Guid SemestreId { get; set; }
        public short? VolumeHorairePrevu { get; set; }
        public DateOnly? DateDebut { get; set; }
        public DateOnly? DateFin { get; set; }
    }

    public class FiliereDto
    {
        public Guid Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Niveau { get; set; }
        public Guid? ResponsableId { get; set; }
    }


    public class PromotionDto
    {
        public Guid Id { get; set; }
        public Guid? FiliereId { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string AnneeScolaire { get; set; } = string.Empty;
        public int? EffectifPrevu { get; set; }
    }

    public class SemestreDto
    {
        public Guid Id { get; set; }
        public Guid AnneeId { get; set; }
        public string Libelle { get; set; } = string.Empty;
        public short Rang { get; set; }
    }
