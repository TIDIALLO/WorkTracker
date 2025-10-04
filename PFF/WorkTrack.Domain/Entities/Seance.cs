using System.ComponentModel.DataAnnotations;

namespace WorkTrack.Domain.Entities;

public class Seance
{
    public Guid Id { get; set; }
    public Guid AffectationId { get; set; }
    public DateTimeOffset Debut { get; set; }
    public DateTimeOffset Fin { get; set; }
    [MaxLength(50)] public string? Salle { get; set; }
    public bool EstVerrouillee { get; set; }
    public Guid CreePar { get; set; }
    public DateTimeOffset CreeLe { get; set; } = DateTimeOffset.UtcNow;
}
