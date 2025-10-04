using System.ComponentModel.DataAnnotations;

namespace WorkTrack.Domain.Entities;

public class Promotion
{
    public Guid Id { get; set; }
    public Guid? FiliereId { get; set; }
    [MaxLength(100)] public string Nom { get; set; } = default!;
    [MaxLength(9)]   public string AnneeScolaire { get; set; } = default!; // "2024-2025"
    public int? EffectifPrevu { get; set; }
}
