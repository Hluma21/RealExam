using System.Security.Claims;
using Greenfield.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Greenfield.Controllers
{

    [Authorize(Roles = "Producer,Producer2,Producer3")]
    public class ProducerDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProducerDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
       public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var producer = await _context.Producer.FirstOrDefaultAsync(p => p.UserId == userId);

            if (producer == null)
            {
                return NotFound();
            }

            var products = await _context.Product.Where(a => a.ProducerId == producer.ProducerId).ToListAsync();

            var orders = await _context.Order.Include(o => o.OrderProducts).ThenInclude(op => op.Products).Where(o => o.OrderProducts.Any(op => op.Products.ProducerId == producer.ProducerId)).ToListAsync();

            ViewBag.TotalPorducts = products.Count;
            ViewBag.LowOnStockCount = products.Count(x => x.stock < 4);
            ViewBag.RecentOrders = orders;

            return View(products);
        }
    }
}
