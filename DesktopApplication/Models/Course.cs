using System;
using System.Collections.Generic;

namespace DesktopApplication.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public virtual List<Group> Groups { get; set; }
}
