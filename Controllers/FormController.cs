using Inventio.Data;
using Inventio.Models;
using Inventio.Models.DTO.Form;
using Inventio.Repositories.Form;
using Microsoft.AspNetCore.Mvc;

namespace Inventio.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class FormController : ControllerBase
  {
    private readonly IFormRepository _formRepository;
    private readonly ApplicationDBContext _context;

    public FormController(IFormRepository formRepository, ApplicationDBContext context)
    {
      _formRepository = formRepository;
      _context = context;
    }

    /* ------------------------- Validation and Handles ------------------------- */

    /* ------------------------------- End points ------------------------------- */

    [HttpGet("select-options")]
    public async Task<ActionResult<DTOSelectOptions>> GetSelectOptions()
    {
      try
      {
        var result = await _formRepository.GetSelectOptions();
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpGet("product-by-line/{idLine}")]
    public async Task<ActionResult<DTOProductByLine>> GetProductByLine(int idLine)
    {
      try
      {
        var result = await _formRepository.GetProductByLine(idLine);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpGet("getCategory")]
    public async Task<ActionResult<List<DowntimeCategory>>> GetCategory()
    {
      try
      {
        var result = await _formRepository.GetCategory();
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpGet("subCategory1/{idCategory}")]
    public async Task<ActionResult<List<DowntimeSubCategory1>>> GetSubCategory1(int idCategory)
    {
      try
      {
        var result = await _formRepository.GetSubcategory1(idCategory);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpGet("subCategory2/{idSubCategory1}")]
    public async Task<ActionResult<List<DowntimeSubCategory2>>> GetSubCategory2(int idSubCategory1)
    {
      try
      {
        var result = await _formRepository.GetSubcategory2(idSubCategory1);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpGet("codes/{idSubCategory2}")]
    public async Task<ActionResult<List<DowntimeCode>>> GetCodes(int idSubCategory2)
    {
      try
      {
        var result = await _formRepository.GetCodes(idSubCategory2);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Productivity>> GetFormById(int id)
    {
      try
      {
        var result = await _formRepository.GetFormById(id);
        return Ok(result);
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateForm(int id, Productivity form)
    {
      try
      {
        await _formRepository.UpdateRecord(id, form);
        return Ok();
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }

    [HttpPost]
    public async Task<ActionResult> CreateForm(Productivity form)
    {
      try
      {
        await _formRepository.CreateRecord(form);
        return Ok();
      }
      catch (Exception ex)
      {
        return StatusCode(500, $"Internal server error: {ex.Message}");
      }
    }
  }
}