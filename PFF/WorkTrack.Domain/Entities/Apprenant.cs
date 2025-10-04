using System.ComponentModel.DataAnnotations;

namespace WorkTrack.Domain.Entities;

public class Apprenant
{
    public Guid Id { get; set; }
    public Guid UtilisateurId { get; set; }
    public Guid? PromotionId { get; set; }
    [MaxLength(50)] public string Matricule { get; set; } = default!;
    public DateTime? DateNaissance { get; set; }
    public StatutApprenant Statut { get; set; } = StatutApprenant.Actif;
}
