using System.ComponentModel.DataAnnotations;

namespace WorkTrack.Domain.Entities;

public class Semestre
{
    public Guid Id { get; set; }
    public Guid AnneeId { get; set; }
    [MaxLength(20)] public string Libelle { get; set; } = default!; // S1, S2
    public short Rang { get; set; }
}
