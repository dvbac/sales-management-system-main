using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesManagement.DAL.Entities
{
    public class LeaveRequest
    {
        [Key]
        public int LeaveRequestId { get; set; }

        public int EmployeeId { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public string Reason { get; set; } = null!;

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public string LeaveType { get; set; } = "Annual"; // Annual, Sick, Unpaid, Maternity, etc.

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public string? AdminComment { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;
    }
}
