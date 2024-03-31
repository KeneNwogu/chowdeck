using System;
using System.Collections.Generic;

namespace Chowdeck.Models;

public partial class RestaurantMenu
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Category { get; set; } = null!;

    public string? Description { get; set; }

    public double Price { get; set; }

    public string? Image { get; set; }

    public string? RestaurantId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Restaurant? Restaurant { get; set; }
}
