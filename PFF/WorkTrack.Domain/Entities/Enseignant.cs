using System.ComponentModel.DataAnnotations;

namespace WorkTrack.Domain.Entities;

public class Enseignant
{
    public Guid Id { get; set; }
    public Guid UtilisateurId { get; set; }
    [MaxLength(150)] public string? Specialite { get; set; }
}
