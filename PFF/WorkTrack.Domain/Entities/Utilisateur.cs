using System.ComponentModel.DataAnnotations;

namespace WorkTrack.Domain.Entities;

public class Utilisateur
{
    public Guid Id { get; set; }
    [MaxLength(100)] public string Prenom { get; set; } = default!;
    [MaxLength(100)] public string Nom { get; set; } = default!;
    [MaxLength(255)] public string Email { get; set; } = default!;
    [MaxLength(30)]  public string? Telephone { get; set; }
    public string MotDePasseHash { get; set; } = default!;
    public RoleUtilisateur Role { get; set; }
    public bool Actif { get; set; } = true;
    public DateTimeOffset CreeLe { get; set; } = DateTimeOffset.UtcNow;
}
