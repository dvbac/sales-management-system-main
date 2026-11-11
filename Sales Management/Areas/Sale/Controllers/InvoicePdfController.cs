using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using SalesManagement.DAL.Data;

namespace SalesManagement.Web.Areas.Sale.Controllers
{
    [Area("Sale")]
    public class InvoicePdfController : Controller
    {
        private readonly AppDbContext _context;

        public InvoicePdfController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Export(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Order)
                    .ThenInclude(o => o.Customer)
                .Include(i => i.Order)
                    .ThenInclude(o => o.OrderDetails)
                        .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null) return NotFound();

            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // ===== TITLE =====
            document.Add(
                new Paragraph()
                    .Add(new Text("INVOICE"))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
            );


            document.Add(new Paragraph($"Invoice #: {invoice.InvoiceId}"));
            document.Add(new Paragraph($"Date: {invoice.InvoiceDate:dd/MM/yyyy}"));
            document.Add(new Paragraph($"Customer: {invoice.Order.Customer.FullName}"));
            document.Add(new Paragraph($"Status: {invoice.Status}"));
            document.Add(new Paragraph("\n"));

            // ===== ORDER TABLE =====
            Table table = new Table(4).UseAllAvailableWidth();
            table.AddHeaderCell("Product");
            table.AddHeaderCell("Quantity");
            table.AddHeaderCell("Price");
            table.AddHeaderCell("Total");

            foreach (var item in invoice.Order.OrderDetails)
            {
                table.AddCell(item.Product.Name);
                table.AddCell(item.Quantity.ToString());
                table.AddCell(item.UnitPrice.ToString("N0"));
                table.AddCell((item.Quantity * item.UnitPrice).ToString("N0"));
            }

            document.Add(table);

            document.Add(new Paragraph($"\nTotal Amount: {invoice.Amount:N0} VND"));
            document.Add(new Paragraph($"Payment Method: {invoice.Order.PaymentMethod}"));
            document.Add(new Paragraph($"Payment Status: {invoice.Order.PaymentStatus}"));

            document.Close();

            return File(
                stream.ToArray(),
                "application/pdf",
                $"Invoice_{invoice.InvoiceId}.pdf"
            );
        }
    }
}
