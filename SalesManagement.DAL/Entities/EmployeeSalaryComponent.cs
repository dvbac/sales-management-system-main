using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SalesManagement.DAL.Entities
{
    public class EmployeeSalaryComponent
    {
        [Key]
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public int SalaryComponentId { get; set; }

        public decimal Amount { get; set; } // Specific amount for this employee (overrides default if needed, or calculated)
        
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        [ForeignKey("SalaryComponentId")]
        public virtual SalaryComponent SalaryComponent { get; set; } = null!;
    }
}
