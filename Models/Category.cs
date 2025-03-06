using System;
using System.Collections.Generic;

namespace CAFE_MENU.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public int? ParentCategoryId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? CreatorUserId { get; set; }

    public virtual ICollection<Category> InverseParentCategory { get; set; } = new List<Category>();

    public virtual Category? ParentCategory { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
