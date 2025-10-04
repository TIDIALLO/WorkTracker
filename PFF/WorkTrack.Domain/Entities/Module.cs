using System.ComponentModel.DataAnnotations;

namespace WorkTrack.Domain.Entities;

public class Module
{
    public Guid Id { get; set; }
    [MaxLength(30)]  public string Code { get; set; } = default!;
    [MaxLength(150)] public string Nom { get; set; } = default!;
    public short? Coefficient { get; set; }
    public short? Ects { get; set; }
}
