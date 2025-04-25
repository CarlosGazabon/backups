using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventio.Models;
using Inventio.Repositories.Settings.LineSetting;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Inventio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LineController : ControllerBase
    {
        private readonly ILineSettingRepository _lineSettingRepository;

        public LineController(ILineSettingRepository lineSettingRepository)
        {
            _lineSettingRepository = lineSettingRepository;
        }


        public struct LineSelect
        {
            public string Label { get; set; }
            public string Value { get; set; }
            public string Color { get; set; }

        }

        // GET: api/Line
        [HttpGet]
        public async Task<ActionResult> GetLine()
        {
            var result = await _lineSettingRepository.GetLines();
            return Ok(result);
        }

        // GET: api/Line/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetLine(int id)
        {
            var line = await _lineSettingRepository.GetLine(id);
            return Ok(line);
        }

        [HttpGet("select-formatter")]
        public async Task<ActionResult> GetLineSelectFormatted()
        {

            var result = await _lineSettingRepository.GetLineSelectFormatted();
            return Ok(result);
        }

        // PUT: api/Line/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLine(int id, Line line)
        {
            if (id != line.Id)
            {
                return BadRequest();
            }

            line.Scrap = Math.Round(line.Scrap, 4);

            try
            {
                var result = await _lineSettingRepository.UpdateLine(line);
                return Ok(result);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_lineSettingRepository.LineExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

        }

        [HttpPut("{id}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            await _lineSettingRepository.ToggleLineInactive(id);
            return Ok();
        }

        // [HttpPut("{id}/toggleCanScrap")]
        // public async Task<IActionResult> ToggleCanScrap(int id)
        // {
        //     var line = await _context.Line.FindAsync(id);

        //     if (line != null)
        //     {
        //         line.CanScrap = !line.CanScrap;
        //         await _context.SaveChangesAsync();
        //     }

        //     return Ok();
        // }

        // [HttpPut("{id}/togglePreformScrap")]
        // public async Task<IActionResult> TogglePreformScrap(int id)
        // {
        //     var line = await _context.Line.FindAsync(id);

        //     if (line != null)
        //     {
        //         line.PreformScrap = !line.PreformScrap;
        //         await _context.SaveChangesAsync();
        //     }

        //     return Ok();
        // }

        // [HttpPut("{id}/toggleBottleScrap")]
        // public async Task<IActionResult> ToggleBottleScrap(int id)
        // {
        //     var line = await _context.Line.FindAsync(id);

        //     if (line != null)
        //     {
        //         line.BottleScrap = !line.BottleScrap;
        //         await _context.SaveChangesAsync();
        //     }

        //     return Ok();
        // }

        // [HttpPut("{id}/togglePouchScrap")]
        // public async Task<IActionResult> TogglePouchScrap(int id)
        // {
        //     var line = await _context.Line.FindAsync(id);

        //     if (line != null)
        //     {
        //         line.PouchScrap = !line.PouchScrap;
        //         await _context.SaveChangesAsync();
        //     }

        //     return Ok();
        // }

        // POST: api/Line
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult> PostLine(Line line)
        {
            var result = await _lineSettingRepository.AddLine(line);
            return Ok(result);
        }

        // DELETE: api/Line/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLine(int id)
        {
            var result = await _lineSettingRepository.DeleteLine(id);
            return Ok(result);
        }
    }
}
