using inventio.Models.DTO;

namespace ProjectNamespace.Utils
{
    public class DTOReactDropdownComparer : IEqualityComparer<DTOReactDropdown<int>>
    {
        public bool Equals(DTOReactDropdown<int>? x, DTOReactDropdown<int>? y)
        {
            if (x == null || y == null)
                return false;

            return x.Value == y.Value; // Compara por Value
        }

        public int GetHashCode(DTOReactDropdown<int> obj)
        {
            return obj.Value.GetHashCode(); // Usa el hash del Value
        }
    }
}
