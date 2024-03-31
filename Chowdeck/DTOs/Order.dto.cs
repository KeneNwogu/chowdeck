using System.ComponentModel.DataAnnotations;

namespace Chowdeck.DTOs
{
    public class CreateOrderDto
    {
        [Required]
        public string RestaurantId { get; set; }

        [Required]
        public List<OrderItemDto> OrderItems { get; set; }

    }

    public class OrderItemDto
    {
        [Required]
        public int Quantity { get; set; }

        [Required]
        public string MenuId { get; set; }
    }
}
