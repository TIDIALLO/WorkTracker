using System.ComponentModel.DataAnnotations;

namespace WorkTrack.Domain.Entities;

public class Filiere
{
    public Guid Id { get; set; }
    [MaxLength(150)] public string Nom { get; set; } = default!;
    [MaxLength(50)]  public string? Niveau { get; set; }
    public Guid? ResponsableId { get; set; }
}
