using System;
using System.Collections.Generic;

namespace DesktopApplication.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public int GroupId { get; set; }

    public string? FirstName { get; set; }

    public string LastName { get; set; } = null!;

    public virtual Group Group { get; set; } = null!;
}
