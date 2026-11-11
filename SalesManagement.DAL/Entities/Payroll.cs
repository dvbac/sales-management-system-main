using System;
using System.Collections.Generic;

namespace SalesManagement.DAL.Entities;

public partial class Payroll
{
    public int PayrollId { get; set; }

    public int EmployeeId { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public decimal BaseSalary { get; set; }

    public decimal Benefits { get; set; }

    public decimal Bonus { get; set; }

    public decimal Penalty { get; set; }

    public decimal TotalAllowances { get; set; }

    public decimal TotalDeductions { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal NetSalary { get; set; }

    public decimal TotalSalary { get; set; }

    public string Status { get; set; } = "Pending"; // Pending, Paid

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    public virtual Employee Employee { get; set; } = null!;
}
