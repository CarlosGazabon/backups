namespace inventio.Models.DTO.Rights
{
    public class DTORightsRequest
    {
        public string MenuId { get; set; } = default!;

        public string RoleId { get; set; } = default!;

        public bool CanView { get; set; } = false;

        public bool CanAdd { get; set; } = false;

        public bool CanDelete { get; set; } = false;

        public bool CanModify { get; set; } = false;
    }
}