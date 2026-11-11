using System.ComponentModel.DataAnnotations;

namespace SalesManagement.DAL.Entities
{
    public class SalaryComponent
    {
        [Key]
        public int SalaryComponentId { get; set; }

        public string Name { get; set; } = null!;

        public string Type { get; set; } = "Allowance"; // "Allowance", "Deduction"

        public decimal DefaultAmount { get; set; }

        public bool IsPercentage { get; set; } // If true, % of BasicSalary

        public string Description { get; set; } = string.Empty;
    }
}
