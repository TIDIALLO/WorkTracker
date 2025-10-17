using WorkTrack.Client.Models;

namespace WorkTrack.Client.Services
{
    public interface IAffectationsService
    {
        Task<List<AffectationModuleDto>> GetAllAsync();
        Task<AffectationModuleDto?> GetByIdAsync(Guid id);
        Task<bool> CreateAsync(AffectationModuleDto affectation);
        Task<bool> UpdateAsync(Guid id, AffectationModuleDto affectation);
        Task<bool> DeleteAsync(Guid id);
    }
}
