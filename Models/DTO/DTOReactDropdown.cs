namespace inventio.Models.DTO
{
    public class DTOReactDropdown<T>
    {
        public string Label { get; set; } = default!;
        public T Value { get; set; } = default!;
    }
}