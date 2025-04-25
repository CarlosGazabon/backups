namespace Inventio.Utils.Formulas
{
  public class EfficiencyFormula
  {
    public decimal CalculateEfficiency(decimal? production, decimal? MaxProduction, bool isPercentage = false, bool round = false, int decimalPlaces = 2)
    {
      var efficiency = production.GetValueOrDefault(0) / Math.Max(MaxProduction.GetValueOrDefault(1), 1);
      if (isPercentage)
      {
        efficiency *= 100;
      }
      if (round)
      {
        efficiency = Math.Round(efficiency, decimalPlaces);
      }
      return efficiency;
    }
  }

}