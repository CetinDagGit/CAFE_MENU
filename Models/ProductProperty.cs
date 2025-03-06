using System;
using System.Collections.Generic;

namespace CAFE_MENU.Models;

public partial class ProductProperty
{
    public long ProductPropertyId { get; set; }

    public long ProductId { get; set; }

    public int PropertyId { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual Property Property { get; set; } = null!;
}
