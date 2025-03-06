using System;
using System.Collections.Generic;

namespace CAFE_MENU.Models;

public partial class Product
{
    public long ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public int CategoryId { get; set; }

    public decimal Price { get; set; }

    public string? ImagePath { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatorUserId { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ProductProperty> ProductProperties { get; set; } = new List<ProductProperty>();
}
