using System;
using System.Collections.Generic;

namespace AuthService.Models;

public partial class Account
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int Role { get; set; }

    public DateTime CreatedAt { get; set; }
}
