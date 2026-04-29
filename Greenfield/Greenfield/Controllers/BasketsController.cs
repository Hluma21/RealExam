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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Authorization;

namespace Greenfield.Controllers
{
    [Authorize] // Only logged-in users can access this controller
    public class BasketsController : Controller
    {
        private readonly ApplicationDbContext _context; // Database connection

        public BasketsController(ApplicationDbContext context)
        {
            _context = context; 
        }

        // GET: Baskets
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the logged-in user's ID

            if (userId == null) // If not logged in
            {
                return Unauthorized();  //return it not being found
            }

            // Look for the user's currently active basket
            var basket = await _context.Basket
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Status);

            if (basket == null) // If no active basket found
            {
                // Create a new empty basket for the user
                basket = new Basket
                {
                    Status = true,
                    UserId = userId,
                };

                _context.Basket.Add(basket); // Add to database
                await _context.SaveChangesAsync(); // Save
            }

            // Get all products in the user's basket, including related data
            var basketProducts = await _context.BasketProduct
                .Where(x => x.BasketId == basket.BasketId)
                .Include(x => x.Basket)
                .Include(x => x.Products)
                .ToListAsync();

            float subtotal = 0f; // Start subtotal at zero

            foreach (var basketProduct in basketProducts) // Loop through each item
            {
                var productTotal = basketProduct.Products.Price * basketProduct.Quaility; // Price × quantity
                subtotal += productTotal; // Add to running subtotal
            }

            // Count how many previous orders the user has placed
            var orderCount = await _context.Order.CountAsync(x => x.UserId == userId);

            float discount = 0f; // Default discount is zero

            if (orderCount >= 3) // If the user has placed 3 or more orders
            {
                discount = subtotal * 0.10f; // Apply a 10% loyalty discount
            }

            float total = subtotal - discount; // Final price after discount

            // Pass pricing info to the view
            ViewBag.Subtotal = subtotal;
            ViewBag.Discount = discount;
            ViewBag.Total = total;
            ViewBag.orderCount = orderCount;

            return View(basketProducts); // Show the basket with all products
        }

        // GET: Baskets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound();  //return it not being found
            }

            // Find the basket by ID
            var basket = await _context.Basket
                .FirstOrDefaultAsync(m => m.BasketId == id);

            if (basket == null) // If not found
            {
                return NotFound();  //return it not being found
            }

            return View(basket); // Show basket details
        }

        // GET: Baskets/Create
        public IActionResult Create()
        {
            return View(); // Show the create form
        }

        // POST: Baskets/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // Protect against CSRF
        public async Task<IActionResult> Create([Bind("BasketId,Status,UserId")] Basket basket)
        {
            if (ModelState.IsValid) // If form data is valid
            {
                _context.Add(basket); // Add the new basket to the database
                await _context.SaveChangesAsync(); // Save it
                return RedirectToAction(nameof(Index)); // Go to the basket list
            }
            return View(basket); // If invalid, show the form again
        }

        // GET: Baskets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound(); //return it not being found
            }

            var basket = await _context.Basket.FindAsync(id); // Find by ID

            if (basket == null) // If not found
            {
                return NotFound();  //return it not being found
            }

            return View(basket); // Show the edit form
        }

        // POST: Baskets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken] // Protect against CSRF
        public async Task<IActionResult> Edit(int id, [Bind("BasketId,Status,UserId")] Basket basket)
        {
            if (id != basket.BasketId) // If the URL ID doesn't match the model
            {
                return NotFound(); //return it not being found
            }

            if (ModelState.IsValid) // If form data is valid
            {
                try
                {
                    _context.Update(basket); // Update the basket
                    await _context.SaveChangesAsync(); // Save changes
                }
                catch (DbUpdateConcurrencyException) // Handle simultaneous edit conflicts
                {
                    if (!BasketExists(basket.BasketId)) // If the basket no longer exists
                    {
                        return NotFound();  //return it not being found
                    }
                    else
                    {
                        throw; // Re-throw for other issues
                    }
                }
                return RedirectToAction(nameof(Index)); // Go back to the list
            }
            return View(basket); // If invalid, show the form again
        }

        // GET: Baskets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound();  //return it not being found
            }

            // Find the basket by ID
            var basket = await _context.Basket
                .FirstOrDefaultAsync(m => m.BasketId == id);

            if (basket == null) // If not found
            {
                return NotFound(); //return it not being found
            }

            return View(basket); // Show delete confirmation page
        }

        // POST: Baskets/Delete/5
        [HttpPost, ActionName("Delete")] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var basket = await _context.Basket.FindAsync(id); // Find the basket

            if (basket != null) // If it exists
            {
                _context.Basket.Remove(basket); // Mark for absent
            }

            await _context.SaveChangesAsync(); // Commit it
            return RedirectToAction(nameof(Index)); // Return to the list
        }

        //Check if a basket with the given ID exists
        private bool BasketExists(int id)
        {
            return _context.Basket.Any(e => e.BasketId == id);
        }
    }
}
