using Inventio.Models;
using Inventio.Models.DTO;

namespace Inventio.Repositories.Security
{
  public interface IFeatureRepository
  {
    Task<IEnumerable<FeatureFlags>> GetFeatures();
    Task<FeatureFlags> AddFeature(FeatureFlags request);
    Task<FeatureFlags> UpdateFeature(int id, FeatureFlags request);
    Task DeleteFeature(int id);
  }

}