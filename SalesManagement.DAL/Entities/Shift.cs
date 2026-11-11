using System;
using System.Collections.Generic;

namespace SalesManagement.DAL.Entities;

public partial class Shift
{
    public int ShiftId { get; set; }

    public string ShiftName { get; set; } = null!;

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();

    public virtual ICollection<TimeAttendance> TimeAttendances { get; set; } = new List<TimeAttendance>();
}
