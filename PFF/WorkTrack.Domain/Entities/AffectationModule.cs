namespace WorkTrack.Domain.Entities;

public class AffectationModule
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public Guid PromotionId { get; set; }
    public Guid EnseignantId { get; set; }
    public Guid SemestreId { get; set; }
    public short? VolumeHorairePrevu { get; set; }
    public DateOnly? DateDebut { get; set; }
    public DateOnly? DateFin { get; set; }
}
