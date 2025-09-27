using System.ComponentModel.DataAnnotations.Schema;

namespace ASP_32.Data.Entities
{
    public class Cart   
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        [Column(TypeName = "decimal(14,2)")]
        public double Price { get; set; }

        public Guid? DiscountId { get; set; }

        public User User { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = [];
        
    }
}
