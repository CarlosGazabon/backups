using inventio.Models.DTO;
using Inventio.Models;
using Inventio.Models.DTO.Settings.Objective;

namespace Inventio.Repositories.Settings.ObjetiveSetting
{
    public interface IObjectiveSettingRepository
    {
        Task<List<DTOObjectiveResponse>> GetObjectives();
        Task<Objective> AddObjective(Objective objectiveRequest);
        Task<Objective> EditObjective(int id, Objective updatedObjective);
        Task<Objective> DeleteObjective(int id);
    }
}