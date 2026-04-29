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
using Microsoft.AspNetCore.Authorization;

namespace Greenfield.Controllers
{
    [Authorize] // Only logged-in users can access this controller
    public class BasketProductsController : Controller
    {
        private readonly ApplicationDbContext _context; // Database connection

        public BasketProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: BasketProducts
        public async Task<IActionResult> Index()
        {
            // Get all basket products, including their related Basket and Product data
            var applicationDbContext = _context.BasketProduct.Include(b => b.Basket).Include(b => b.Products);
            return View(await applicationDbContext.ToListAsync()); // Pass the list to the view
        }

        // GET: BasketProducts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) // If no ID was provided
            {
                return NotFound(); //return it not being found
            }

            // Find the basket product by ID, including related Basket and Product
            var basketProduct = await _context.BasketProduct
                .Include(b => b.Basket)
                .Include(b => b.Products)
                .FirstOrDefaultAsync(m => m.BasketProductId == id);

            if (basketProduct == null) // If no match was found
            {
                return NotFound();  //return it not being found
            }

            return View(basketProduct); // Pass the found item to the view
        }

        // GET: BasketProducts/Create
        public IActionResult Create()
        {
            // dropdown for basket selection
            ViewData["BasketId"] = new SelectList(_context.Basket, "BasketId", "BasketId");
            // dropdown for product selection
            ViewData["ProductId"] = new SelectList(_context.Set<Product>(), "ProductId", "ProductId");
            return View(); // Show the create form
        }

        // POST: BasketProducts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int ProductId)
        {
            // Try to find the product with the given ID
            var product = await _context.Product.FirstOrDefaultAsync(x => x.ProductId == ProductId);

            if (product == null) // If product doesn't exist
            {
                return NotFound();  //return it not being found
            }

            // Get the current logged-in user's ID
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null) // If user isn't logged in
            {
                return Unauthorized();  //return it not being found
            }

            // Look for the user's active basket
            var basket = await _context.Basket.FirstOrDefaultAsync(x => x.UserId == userId && x.Status == true);

            if (basket == null) // If no active basket exists
            {
                // Create a new active basket for the user
                basket = new Basket
                {
                    Status = true,
                    UserId = userId
                };

                _context.Basket.Add(basket); // Add it to the database
                await _context.SaveChangesAsync(); // Save it
            }

            // Check if this product is already in the basket
            var basketProduct = await _context.BasketProduct
                .FirstOrDefaultAsync(bp => bp.BasketId == basket.BasketId && bp.ProductId == ProductId);

            if (basketProduct != null) // If the product is already in the basket
            {
                basketProduct.Quaility++; // Increase the quantity by 1
            }
            else // If it's not in the basket yet
            {
                // Create a new basket product entry
                basketProduct = new BasketProduct
                {
                    BasketId = basket.BasketId,
                    ProductId = ProductId,
                    Quaility = 1, // Start with quantity of 1
                };

                _context.BasketProduct.Add(basketProduct); // Add to the database
            }

            await _context.SaveChangesAsync(); // Save all changes
            return RedirectToAction("Index", "Baskets"); // Go to the basket page
        }

        // GET: BasketProducts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound();  //return it not being found
            }

            var basketProduct = await _context.BasketProduct.FindAsync(id); // Find by ID

            if (basketProduct == null) // If not found
            {
                return NotFound(); //return it not being found
            }

            // Populate dropdowns with current values pre-selected
            ViewData["BasketId"] = new SelectList(_context.Basket, "BasketId", "BasketId", basketProduct.BasketId);
            ViewData["ProductId"] = new SelectList(_context.Set<Product>(), "ProductId", "ProductId", basketProduct.ProductId);
            return View(basketProduct); // Show the edit form
        }

        // POST: BasketProducts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken] // Protect against CSRF
        public async Task<IActionResult> Edit(int id, [Bind("BasketProductId,BasketId,ProductId,Quaility")] BasketProduct basketProduct)
        {
            if (id != basketProduct.BasketProductId) // If the URL ID doesn't match the model ID
            {
                return NotFound(); //return it not being found
            }

            if (ModelState.IsValid) // If the submitted form data is valid
            {
                try
                {
                    _context.Update(basketProduct); // Update the record in the database
                    await _context.SaveChangesAsync(); // Save changes
                }
                catch (DbUpdateConcurrencyException) // If another user edited at the same time
                {
                    if (!BasketProductExists(basketProduct.BasketProductId)) // If the record no longer exists
                    {
                        return NotFound();  //return it not being found
                    }
                    else
                    {
                        throw; 
                    }
                }
                return RedirectToAction(nameof(Index)); // Go back to the list
            }

            
            ViewData["BasketId"] = new SelectList(_context.Basket, "BasketId", "BasketId", basketProduct.BasketId);
            ViewData["ProductId"] = new SelectList(_context.Set<Product>(), "ProductId", "ProductId", basketProduct.ProductId);
            return View(basketProduct);
        }

        // GET: BasketProducts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound(); //return it not being found
            }

            // Find the basket product with related data for display
            var basketProduct = await _context.BasketProduct
                .Include(b => b.Basket)
                .Include(b => b.Products)
                .FirstOrDefaultAsync(m => m.BasketProductId == id);

            if (basketProduct == null) // If not found
            {
                return NotFound(); //return it not being found
            }

            return View(basketProduct); // Show the delete confirmation page
        }

        // POST: BasketProducts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var basketProduct = await _context.BasketProduct.FindAsync(id); // Find the record by ID

            if (basketProduct != null) // If it exists
            {
                _context.BasketProduct.Remove(basketProduct); // Mark as asbent
            }

            await _context.SaveChangesAsync(); // Commit it
            return RedirectToAction(nameof(Index)); // Go back to the list
        }

        // Helper: Check if a basket product with the given ID exists
        private bool BasketProductExists(int id)
        {
            return _context.BasketProduct.Any(e => e.BasketProductId == id);
        }
    }
}