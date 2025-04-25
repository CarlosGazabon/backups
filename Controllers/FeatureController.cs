using Inventio.Models;
using Inventio.Models.DTO;
using Inventio.Repositories.Security;
using Microsoft.AspNetCore.Mvc;

namespace Inventio.Controllers
{
  [ApiController]
  [Route("api/feature-flags")]
  public class FeatureController : ControllerBase
  {

    private readonly IFeatureRepository _featureRepository;
    public FeatureController(IFeatureRepository featureRepository)
    {
      _featureRepository = featureRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeatureFlags>>> GetFeatures()
    {
      var features = await _featureRepository.GetFeatures();
      return Ok(features);
    }

    [HttpPost]
    public async Task<ActionResult<FeatureFlags>> AddFeature(FeatureFlags request)
    {
      Console.WriteLine("Request: " + request);
      var feature = await _featureRepository.AddFeature(request);
      return CreatedAtAction(nameof(GetFeatures), new { id = feature.Id }, feature);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateFeature(int id, FeatureFlags request)
    {
      var feature = await _featureRepository.UpdateFeature(id, request);
      return Ok(feature);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
      await _featureRepository.DeleteFeature(id);
      return NoContent();
    }
  }
}