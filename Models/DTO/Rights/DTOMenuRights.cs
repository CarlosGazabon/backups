namespace inventio.Models.DTO.Rights
{
    public class DTOMenuRights
    {
        public string Id { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string ParentLabel { get; set; } = default!;
        public bool? CanView { get; set; } = false;
        public bool? CanAdd { get; set; } = false;
        public bool? CanDelete { get; set; } = false;
        public bool? CanModify { get; set; } = false;

    }
}