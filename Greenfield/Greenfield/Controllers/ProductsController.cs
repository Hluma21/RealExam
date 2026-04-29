using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Greenfield.Data;
using Greenfield.Models;
using System.Security.Claims;

namespace Greenfield.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context; // Database connection

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("producer")) // If the user is a producer
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get their user ID

                if (userId == null) // If not logged in
                {
                    return Unauthorized(); //return it not being found
                }

                // Find the producer record linked to this user
                var producer = await _context.Producer.FirstOrDefaultAsync(p => p.UserId == userId);

                if (producer == null) // If no producer record found
                {
                    return NotFound(); //return it not being found
                }

                // Get only the products belonging to this producer
                var producerProducts = await _context.Product.Where(p => p.ProducerId == producer.ProducerId).Include(p => p.ProducerId).ToListAsync();
                return View(producerProducts); // Show only their products
            }
            else // For all other users (customers, admins, etc.)
            {
                // Get all products with their producer info
                var allProducts = await _context.Product.Include(p => p.Producer).ToListAsync();
                return View(allProducts); // Show all products
            }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound(); //return it not being found
            }

            // Find the product by ID, including its producer
            var product = await _context.Product
                .Include(p => p.Producer)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) // If not found
            {
                return NotFound(); //return it not being found
            }

            return View(product); // Show product details
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View(); // Show the create form
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProducerId,ProductName,Description,Rating,stock,IsAvailable,Price")] Product product)
        {
            if (ModelState.IsValid) // If form data is valid
            {
                _context.Add(product); // Add the product to the database
                await _context.SaveChangesAsync(); // Save
                return RedirectToAction(nameof(Index)); // Go to the list
            }

            // If invalid, repopulate producer dropdown and show form again
            ViewData["ProducerId"] = new SelectList(_context.Producer, "ProducerId", "ProducerId", product.ProducerId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound(); //return it not being found
            }

            var product = await _context.Product.FindAsync(id); // Find the product

            if (product == null) // If not found
            {
                return NotFound(); //return it not being found
            }

            //  producer dropdown with current value pre-selected
            ViewData["ProducerId"] = new SelectList(_context.Producer, "ProducerId", "ProducerId", product.ProducerId);
            return View(product); // Show the edit form
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Description,Rating,stock,IsAvailable,Price")] Product product)
        {
            if (id != product.ProductId) // If URL ID doesn't match model ID
            {
                return NotFound();//return it not being found
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user's ID

            if (userId == null) // If not logged in
            {
                return Unauthorized(); //return it not being found
            }

            // Find the producer linked to this user
            var producer = await _context.Producer.FirstOrDefaultAsync(P => P.UserId == userId);

            if (producer == null) // If not found
            {
                return NotFound(); //return it not being found
            }

            if (ModelState.IsValid) // If form data is valid
            {
                try
                {
                    _context.Update(product); // Update the product
                    await _context.SaveChangesAsync(); // Save changes
                }
                catch (DbUpdateConcurrencyException) // Handle edit conflicts
                {
                    if (!ProductExists(product.ProductId)) // If product no longer exists
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

            // If invalid,show form again
            ViewData["ProducerId"] = new SelectList(_context.Producer, "ProducerId", "ProducerId", product.ProducerId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound(); //return it not being found
            }

            // Find the product with its producer for display
            var product = await _context.Product
                .Include(p => p.Producer)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null) // If not found
            {
                return NotFound(); //return it not being found
            }

            return View(product); // Show delete confirmation
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id); // Find the product

            if (product != null) // If it exists
            {
                _context.Product.Remove(product); // Mark for absence
            }

            await _context.SaveChangesAsync(); // Commit it
            return RedirectToAction(nameof(Index)); // Return to list
        }

        //Check if a product with the given ID exists
        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}
