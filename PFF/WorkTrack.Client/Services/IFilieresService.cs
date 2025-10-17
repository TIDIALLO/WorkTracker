namespace WorkTrack.Client.Services.Interfaces;
using WorkTrack.Client.Models;

public interface IFilieresService
{
    Task<List<FiliereDto>> GetAllAsync();
    Task<FiliereDto?> GetByIdAsync(Guid id);
    Task<bool> CreateAsync(FiliereDto filiere);
    Task<bool> UpdateAsync(Guid id, FiliereDto filiere);
    Task<bool> DeleteAsync(Guid id);
}
