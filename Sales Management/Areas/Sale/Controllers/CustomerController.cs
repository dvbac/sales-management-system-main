using Microsoft.AspNetCore.Mvc;
using SalesManagement.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using SalesManagement.Web.Areas.Sale.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagement.Web.Areas.Sale.Controllers
{
    [Area("Sale")]
    [Authorize(Roles = "Sales, Admin")]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        // GET: Sales/Customer
        public async Task<IActionResult> Index(string searchString, int? pageNumber)
        {
            var customers = await _customerService.GetAllCustomersAsync(searchString);
            ViewData["CurrentFilter"] = searchString;

            int pageSize = 10;
            return View(PaginatedList<SalesManagement.DAL.Entities.Customer>.Create(customers.AsQueryable(), pageNumber ?? 1, pageSize));
        }

        // GET: Sales/Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _customerService.GetCustomerDetailsAsync(id.Value);
            if (customer == null) return NotFound();

            return View(customer);
        }
    }
}
