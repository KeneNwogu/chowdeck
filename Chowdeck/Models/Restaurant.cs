using System;
using System.Collections.Generic;

namespace Chowdeck.Models;

public partial class Restaurant
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public int Rank { get; set; }

    public double? AverageRating { get; set; }

    public int? Ratings { get; set; }

    public string? Image { get; set; }

    public string Address { get; set; } = null!;

    public string Lat { get; set; } = null!;

    public string Lng { get; set; } = null!;

    public string Category { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<RestaurantMenu> RestaurantMenus { get; set; } = new List<RestaurantMenu>();
}
