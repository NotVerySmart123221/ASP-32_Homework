using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ASP_32.Data.Entities
{
    public record Product
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public String Name { get; set; } = null!;
        public String? Description { get; set; } = null!;
        public String? Slug { get; set; } = null!;
        public String? ImageUrl { get; set; } = null!;
        public int Stock { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public double Price { get; set; }
        public DateTime? DeletedAt { get; set; }

        [JsonIgnore]
        public ProductGroup Group { get; set; } = null!;

        public ICollection<Feedback> Feedbacks { get; set; }

        public int FeedbackCount { get; set; }
    }
}
