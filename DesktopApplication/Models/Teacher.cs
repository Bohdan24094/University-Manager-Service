using System;
using System.Collections.Generic;

namespace DesktopApplication.Models;

public partial class Teacher
{
    public int TeacherId { get; set; }

    public string? FirstName { get; set; }

    public string LastName { get; set; } = null!;

    public Group Group { get; set; } = null!;
}
