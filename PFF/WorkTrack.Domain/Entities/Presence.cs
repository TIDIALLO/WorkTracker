using System.ComponentModel.DataAnnotations;

namespace WorkTrack.Domain.Entities;

public class Presence
{
    public Guid Id { get; set; }
    public Guid SeanceId { get; set; }
    public Guid ApprenantId { get; set; }
    public StatutPresence Statut { get; set; }
    public short? MinutesRetard { get; set; }
    public bool Justifie { get; set; }
    [MaxLength(500)] public string? Commentaire { get; set; }
    public Guid MarquePar { get; set; }
    public DateTimeOffset MarqueLe { get; set; } = DateTimeOffset.UtcNow;
}
