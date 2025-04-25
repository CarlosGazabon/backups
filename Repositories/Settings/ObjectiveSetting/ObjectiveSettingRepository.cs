using inventio.Models.DTO;
using Inventio.Data;
using Inventio.Models;
using Inventio.Models.DTO.Settings.Objective;
using Microsoft.EntityFrameworkCore;

namespace Inventio.Repositories.Settings.ObjetiveSetting
{
    public class ObjectiveSettingRepository : IObjectiveSettingRepository
    {
        private readonly ApplicationDBContext _context;

        public ObjectiveSettingRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<DTOObjectiveResponse>> GetObjectives()
        {
            var objectives = await _context.Objective.Include(o => o.Line).ToListAsync();

            var response = objectives.Select(o => new DTOObjectiveResponse
            {
                Id = o.Id,
                Time = o.Time,
                LineID = o.Line.Id,
                Production = o.Production,
                Utilization = o.Utilization,
                Efficiency = o.Efficiency,
                Scrap = o.Scrap,
                Line = new DTOReactDropdown<int>
                {
                    Label = o.Line.Name,
                    Value = o.Line.Id
                }
            }).ToList();

            return response;
        }

        public async Task<Objective> AddObjective(Objective objective)
        {
            var line = await _context.Line.FindAsync(objective.LineID) ?? throw new Exception($"Objective with id {objective.LineID} not found");
            objective.Line = line;
            _context.Objective.Add(objective);
            await _context.SaveChangesAsync();
            return objective;
        }


        public async Task<Objective> EditObjective(int id, Objective updatedObjective)
        {
            try
            {
                var line = await _context.Line.FindAsync(updatedObjective.LineID) ?? throw new Exception($"Objective with id {updatedObjective.LineID} not found");
                updatedObjective.Line = line;
                var existingObjective = await _context.Objective.FindAsync(updatedObjective.Id) ?? throw new Exception($"Objective with id {updatedObjective.Id} not found");
                existingObjective.Time = updatedObjective.Time;
                existingObjective.LineID = updatedObjective.LineID;
                existingObjective.Production = updatedObjective.Production;
                existingObjective.Utilization = updatedObjective.Utilization;
                existingObjective.Efficiency = updatedObjective.Efficiency;
                existingObjective.Scrap = updatedObjective.Scrap;
                existingObjective.Line = updatedObjective.Line;


                await _context.SaveChangesAsync();
                return existingObjective;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating objective: {ex.Message}");
            }
        }

        public async Task<Objective> DeleteObjective(int id)
        {
            try
            {
                var objective = await _context.Objective.FindAsync(id) ?? throw new Exception($"Objective with id {id} not found");

                _context.Objective.Remove(objective);
                await _context.SaveChangesAsync();

                return objective;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting objective: {ex.Message}");
            }
        }
    }
}
