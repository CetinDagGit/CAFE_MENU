using System;
using System.Collections.Generic;

namespace CAFE_MENU.Models;

public partial class Property
{
    public int PropertyId { get; set; }

    public string Key { get; set; } = null!;

    public string Value { get; set; } = null!;

    public virtual ICollection<ProductProperty> ProductProperties { get; set; } = new List<ProductProperty>();
}
