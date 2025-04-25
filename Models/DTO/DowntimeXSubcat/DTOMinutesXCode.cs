namespace inventio.Models.DTO.DowntimeXSubcat
{
    public struct DTOMinutesXCode
    {
        public string Code { get; set; }
        public string SubCategory { get; set; }
        public string Failure { get; set; }
        public decimal? Minutes { get; set; }
    }
}