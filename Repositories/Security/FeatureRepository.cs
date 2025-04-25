using Inventio.Data;
using Inventio.Models;
using Inventio.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Inventio.Repositories.Security
{
  public class FeatureRepository : IFeatureRepository
  {
    private readonly ApplicationDBContext _context;

    public FeatureRepository(ApplicationDBContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<FeatureFlags>> GetFeatures()
    {
      return await _context.FeatureFlags.ToListAsync();
    }
    public async Task<FeatureFlags> AddFeature(FeatureFlags request)
    {
      var existingFeature = await _context.FeatureFlags.FirstOrDefaultAsync(ff => ff.Code == request.Code);

      if (existingFeature != null)
      {
        throw new InvalidOperationException($"A feature flag with code {request.Code} already exists.");
      }
      _context.FeatureFlags.Add(request);
      await _context.SaveChangesAsync();
      return request;
    }
    public async Task<FeatureFlags> UpdateFeature(int id, FeatureFlags request)
    {
      if (id != request.Id)
      {
        throw new Exception("Id mismatch");
      }

      var existingFeature = await _context.FeatureFlags.FindAsync(id);
      if (existingFeature == null)
      {
        throw new Exception("Feature flag not found");
      }

      if (existingFeature.Code != request.Code)
      {
        var duplicateFeature = await _context.FeatureFlags
            .FirstOrDefaultAsync(ff => ff.Code == request.Code);
        if (duplicateFeature != null)
        {
          throw new InvalidOperationException($"A feature flag with code {request.Code} already exists.");
        }
      }

      existingFeature.Name = request.Name;
      existingFeature.Code = request.Code;
      existingFeature.Inactive = request.Inactive;

      await _context.SaveChangesAsync();
      return existingFeature;
    }
    
    public async Task DeleteFeature(int id)
    {
      var feature = await _context.FeatureFlags.FindAsync(id) ?? throw new Exception("Feature not found");
      _context.FeatureFlags.Remove(feature);
      await _context.SaveChangesAsync();
    }
  }
}
