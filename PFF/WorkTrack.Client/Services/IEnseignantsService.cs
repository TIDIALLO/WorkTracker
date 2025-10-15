using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkTrack.Client.Models;

namespace WorkTrack.Client.Services
{
    public interface IEnseignantsService
    {
        Task<List<EnseignantDto>> GetAllAsync();
        Task<EnseignantDto?> GetByIdAsync(Guid id);
        Task<bool> CreateAsync(EnseignantCreateOrUpdateDto enseignant);
        Task<bool> UpdateAsync(Guid id, EnseignantCreateOrUpdateDto enseignant);
        Task<bool> DeleteAsync(Guid id);
    }
}
    