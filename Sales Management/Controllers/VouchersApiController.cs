using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesManagement.DAL.Data;
using SalesManagement.DAL.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SalesManagement.Web.Controllers.Api
{
    [Route("api/vouchers")]
    [ApiController]
    public class VouchersApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VouchersApiController> _logger;

        public VouchersApiController(AppDbContext context, ILogger<VouchersApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/vouchers/check-expired
        [HttpGet("check-expired")]
        public async Task<IActionResult> CheckExpired()
        {
            var now = DateTime.Now;
            var expiredCandidates = await _context.Promotions
                .Where(p => p.Status == "Active" && p.EndDate < now)
                .ToListAsync();

            return Ok(new
            {
                Count = expiredCandidates.Count,
                Message = $"{expiredCandidates.Count} vouchers found that are effectively expired but status is Active.",
                Vouchers = expiredCandidates.Select(v => new { v.PromotionId, v.Code, v.EndDate })
            });
        }

        // PATCH: api/vouchers/5/status
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var voucher = await _context.Promotions.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }

            if (status != "Active" && status != "Expired" && status != "Disabled")
            {
                return BadRequest("Invalid status. Allowed values: Active, Expired, Disabled.");
            }

            // Log change
            _logger.LogInformation($"Manual status update for Voucher {voucher.Code} (ID: {id}) from {voucher.Status} to {status}.");

            voucher.Status = status;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Status updated successfully.", Voucher = voucher });
        }
    }
}
