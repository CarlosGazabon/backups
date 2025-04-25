using Microsoft.AspNetCore.Mvc;
using Inventio.Models;
using Inventio.Repositories.Settings.ObjetiveSetting;
using inventio.Models.DTO;
using Inventio.Models.DTO.Settings.Objective;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ObjectiveController : ControllerBase
    {
        private readonly IObjectiveSettingRepository _objectiveSettingRepository;

        public ObjectiveController(IObjectiveSettingRepository objectiveSettingRepository)
        {
            _objectiveSettingRepository = objectiveSettingRepository;
        }

        // GET: api/Objective
        [HttpGet]
        public async Task<ActionResult> GetObjectives()
        {
            try
            {
                var result = await _objectiveSettingRepository.GetObjectives();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Objective/5
        // [HttpGet("{id}")]
        // public async Task<ActionResult> GetObjective(int id)
        // {
        //     try
        //     {
        //         var objective = await _objectiveSettingRepository.GetObjective(id);
        //         if (objective == null)
        //         {
        //             return NotFound();
        //         }
        //         return Ok(objective);
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, $"Internal server error: {ex.Message}");
        //     }
        // }

        // POST: api/Objective
        [HttpPost]
        public async Task<ActionResult> PostObjective(DTOObjectiveRequest request)
        {
            try
            {
                Objective objective = new()
                {
                    Time = request.Time,
                    LineID = request.LineID,
                    Production = request.Production,
                    Utilization = request.Utilization,
                    Efficiency = request.Efficiency,
                    Scrap = request.Scrap,
                    Line = new()
                    {
                        Id = request.LineID,
                        Name = ""
                    }
                };

                var createdObjective = await _objectiveSettingRepository.AddObjective(objective);

                return Ok(createdObjective);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Objective/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutObjective(int id, DTOObjectiveRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest();
            }

            try
            {
                Objective objective = new()
                {
                    Id = request.Id,
                    Time = request.Time,
                    LineID = request.LineID,
                    Production = request.Production,
                    Utilization = request.Utilization,
                    Efficiency = request.Efficiency,
                    Scrap = request.Scrap,
                    Line = new()
                    {
                        Id = request.LineID,
                        Name = ""
                    }
                };
                var result = await _objectiveSettingRepository.EditObjective(id, objective);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Objective/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteObjective(int id)
        {
            try
            {
                var result = await _objectiveSettingRepository.DeleteObjective(id);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
