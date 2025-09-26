namespace ASP_32.Data.Entities
{
    public class Feedback
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid? UserId { get; set; }
        public int? Rate { get; set; }
        public String? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
