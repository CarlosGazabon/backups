using Microsoft.AspNetCore.Mvc;
using Inventio.Repositories.Settings.HoursSetting;
using Inventio.Models.DTO.Settings.Hours;
using Inventio.Models;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HourController : ControllerBase
    {
        private readonly IHoursSettingRepository _hoursSettingRepository;

        public HourController(IHoursSettingRepository hoursSettingRepository)
        {
            _hoursSettingRepository = hoursSettingRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetHours()
        {
            try
            {
                var result = await _hoursSettingRepository.GetHours();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddHours(DTOHoursRequest request)
        {
            try
            {
                var result = await _hoursSettingRepository.AddHours(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> EditHours(DTOHoursRequest request)
        {
            try
            {
                var updatedHours = await _hoursSettingRepository.EditHours(request);
                return Ok(updatedHours);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteHours()
        {
            try
            {
                var result = await _hoursSettingRepository.DeleteHours();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}
