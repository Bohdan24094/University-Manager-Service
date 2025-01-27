﻿using System;
using System.Collections.Generic;

namespace DesktopApplication.Models;

public partial class Group
{
    public int GroupId { get; set; }

    public int CourseId { get; set; }

    public int TeacherId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;

    public virtual List<Student> Students { get; set; }

    public virtual Teacher Teacher { get; set; } = null!;
}
