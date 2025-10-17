using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkTrack.Client.Models;

namespace WorkTrack.Client.Services
{
    public interface IModulesService
    {
        Task<List<ModuleDto>> GetAllAsync();
        Task<ModuleDto?> GetByIdAsync(Guid id);
        Task<bool> CreateAsync(ModuleDto module);
        Task<bool> UpdateAsync(Guid id, ModuleDto module);
        Task<bool> DeleteAsync(Guid id);
    }
}
