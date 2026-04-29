using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Greenfield.Data;
using Greenfield.Models;

namespace Greenfield.Controllers
{
    public class OrderProductsController : Controller
    {
        private readonly ApplicationDbContext _context; 

        public OrderProductsController(ApplicationDbContext context)
        {
            _context = context; 
        }

        // GET: OrderProducts
        public async Task<IActionResult> Index()
        {
            // Get all order products, including their related Order and Product data
            var applicationDbContext = _context.OrderProduct.Include(o => o.Orders).Include(o => o.Products);
            return View(await applicationDbContext.ToListAsync()); // Pass list to view
        }

        // GET: OrderProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound(); //return it not being found
            }

            // Find order product by ID with related data
            var orderProduct = await _context.OrderProduct
                .Include(o => o.Orders)
                .Include(o => o.Products)
                .FirstOrDefaultAsync(m => m.OrderProductId == id);

            if (orderProduct == null) // If not found
            {
                return NotFound();//return it not being found
            }

            return View(orderProduct); // Show details
        }

        // GET: OrderProducts/Create
        public IActionResult Create()
        {
            // Populate order and product dropdowns
            ViewData["OrderId"] = new SelectList(_context.Order, "OrderId", "OrderId");
            ViewData["ProductId"] = new SelectList(_context.Set<Product>(), "ProductId", "ProductId");
            return View(); // Show the create form
        }

        // POST: OrderProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderProductId,OrderId,ProductId,Quantity")] OrderProduct orderProduct)
        {
            if (ModelState.IsValid) // If form data is valid
            {
                _context.Add(orderProduct); // Add to database
                await _context.SaveChangesAsync(); // Save
                return RedirectToAction(nameof(Index)); // Go to list
            }

            // If invalid, repopulate dropdowns and show form again
            ViewData["OrderId"] = new SelectList(_context.Order, "OrderId", "OrderId", orderProduct.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Set<Product>(), "ProductId", "ProductId", orderProduct.ProductId);
            return View(orderProduct);
        }

        // GET: OrderProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound();//return it not being found
            }

            var orderProduct = await _context.OrderProduct.FindAsync(id); // Find by ID

            if (orderProduct == null) // If not found
            {
                return NotFound(); //return it not being found
            }

            // Populate dropdowns with current values pre-selected
            ViewData["OrderId"] = new SelectList(_context.Order, "OrderId", "OrderId", orderProduct.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Set<Product>(), "ProductId", "ProductId", orderProduct.ProductId);
            return View(orderProduct); // Show the edit form
        }

        // POST: OrderProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Edit(int id, [Bind("OrderProductId,OrderId,ProductId,Quantity")] OrderProduct orderProduct)
        {
            if (id != orderProduct.OrderProductId) // If URL ID doesn't match model ID
            {
                return NotFound();//return it not being found
            }

            if (ModelState.IsValid) // If form data is valid
            {
                try
                {
                    _context.Update(orderProduct); // Update the record
                    await _context.SaveChangesAsync(); // Save changes
                }
                catch (DbUpdateConcurrencyException) // Handle edit conflicts
                {
                    if (!OrderProductExists(orderProduct.OrderProductId)) // If record no longer exists
                    {
                        return NotFound(); //return it not being found
                    }
                    else
                    {
                        throw; 
                    }
                }
                return RedirectToAction(nameof(Index)); // Go back to list
            }

            // If invalid, repopulate dropdowns and show form again
            ViewData["OrderId"] = new SelectList(_context.Order, "OrderId", "OrderId", orderProduct.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Set<Product>(), "ProductId", "ProductId", orderProduct.ProductId);
            return View(orderProduct);
        }

        // GET: OrderProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound();//return it not being found
            }

            // Find the order product with related data for display
            var orderProduct = await _context.OrderProduct
                .Include(o => o.Orders)
                .Include(o => o.Products)
                .FirstOrDefaultAsync(m => m.OrderProductId == id);

            if (orderProduct == null) // If not found
            {
                return NotFound();//return it not being found
            }

            return View(orderProduct); // Show delete confirmation
        }

        // POST: OrderProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orderProduct = await _context.OrderProduct.FindAsync(id); // Find by ID

            if (orderProduct != null) // If it exists
            {
                _context.OrderProduct.Remove(orderProduct); // Mark for absent
            }

            await _context.SaveChangesAsync(); // Commit it
            return RedirectToAction(nameof(Index)); // Return to list
        }

        //Check if an order product with the given ID exists
        private bool OrderProductExists(int id)
        {
            return _context.OrderProduct.Any(e => e.OrderProductId == id);
        }
    }
}
