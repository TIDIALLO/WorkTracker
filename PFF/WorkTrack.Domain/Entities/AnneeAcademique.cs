using System.ComponentModel.DataAnnotations;

namespace WorkTrack.Domain.Entities;

public class AnneeAcademique
{
    public Guid Id { get; set; }
    [MaxLength(20)] public string Libelle { get; set; } = default!;
    public DateOnly DateDebut { get; set; }
    public DateOnly DateFin { get; set; }
}
