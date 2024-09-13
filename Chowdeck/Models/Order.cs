using NuGet.Versioning;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;

namespace Chowdeck.Models
{
    public enum OrderTimelineStageEnum
    {
        OrderReceived = 1,
        PreparingOrder = 2,
        RiderAcceptedOrder = 3,
        RiderAtVendor = 4,
        RiderPickedUpOrder = 5,
        OrderArrived = 6,
        OrderDelivered = 7
    }
    public class Order
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public double TotalAmount { get; set; }

        [Required]
        public double ServiceCharge { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string RestaurantId { get; set; }

        [Required]
        public string PaymentStatus { get; set; }

        public string? RiderId { get; set; } = null;

        
        public virtual Restaurant? Restaurant { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<OrderTimeline> Timeline { get; set; } = new List<OrderTimeline>();
    }

    public class OrderItem
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public double Amount { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public string MenuId { get; set; }

        [Required]
        public string OrderId { get; set; }

        //[ForeignKey("OrderId")]
        //public virtual Order Order { get; set; }

        //[ForeignKey("MenuId")]
        public virtual RestaurantMenu? Menu { get; set; }
    }

    public class OrderTimeline
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public OrderTimelineStageEnum Stage { get; set; }

        [Required]
        public string OrderId { get; set; }

        [Required]
        public string Name { get; set; }

        public string? RiderId { get; set; }

        [Required]
        public bool Completed { get; set; }

        [ForeignKey("RiderId")]
        public virtual User? Rider { get; set; }

        //[ForeignKey("OrderId")]
        //public virtual Order Order { get; set; }
    }
}
