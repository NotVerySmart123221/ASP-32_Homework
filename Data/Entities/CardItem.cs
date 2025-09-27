using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP_32.Data.Entities
{
    public class CartItem
    {
        public Guid Id { get; set; }
        public Guid CartId { get; set; }
        public Guid ProductId { get; set; }

        [Column(TypeName = "decimal(14,2)")]
        public decimal Price { get; set; }

        public Guid? DiscountId { get; set; }

        public Cart Cart { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }
    }
}
