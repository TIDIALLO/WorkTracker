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
