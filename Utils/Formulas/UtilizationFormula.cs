namespace Inventio.Utils.Formulas
{
    public class UtilizationFormula
    {
        public decimal DetailedUtilizationFormula(decimal productionHrs, decimal excludedCategoryHrs, int numberOfLines, int workingHours, int workingDays)
        {
            int timePeriod = numberOfLines * workingHours * workingDays;

            if (timePeriod == 0)
            {
                return 0;
            }

            return (productionHrs - excludedCategoryHrs) / timePeriod;
        }

        public decimal ShortUtilizationFormula(decimal productionHrs, decimal excludedCategoryHrs, int timePeriod)
        {
            if (timePeriod == 0)
            {
                return 0;
            }

            return (productionHrs - excludedCategoryHrs) / timePeriod;
        }
    }
}