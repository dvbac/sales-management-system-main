using System;
using System.Collections.Generic;

namespace SalesManagement.DAL.Entities;

public partial class TimeAttendance
{
    public int AttendanceId { get; set; }

    public int EmployeeId { get; set; }

    public int? ShiftId { get; set; }

    public DateOnly Date { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? CheckOutTime { get; set; }

    public string? Status { get; set; } // Present, Absent, Late, LeftEarly

    public string? Platform { get; set; } // Web, Mobile

    public string? Notes { get; set; }

    public double WorkHours { get; set; }

    public double OvertimeHours { get; set; }



    public int MinutesLate { get; set; }

    public decimal DeductionAmount { get; set; }

    public virtual Employee Employee { get; set; } = null!;

    public virtual Shift? Shift { get; set; }
}
