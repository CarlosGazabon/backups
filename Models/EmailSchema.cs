
namespace inventio.Models
{
    public class EmailSchema
    {
        public required string Receiver { get; set; }
        public required string Subject { get; set; }
        public required string Message { get; set; }
    }
}