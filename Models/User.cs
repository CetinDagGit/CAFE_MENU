using System;
using System.Collections.Generic;

namespace CAFE_MENU.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string UserName { get; set; } = null!;

    public byte[] HashPassword { get; set; } = null!;

    public byte[] SaltPassword { get; set; } = null!;
}
