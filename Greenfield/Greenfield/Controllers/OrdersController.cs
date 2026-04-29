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
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context; // Database connection

        public OrdersController(ApplicationDbContext context)
        {
            _context = context; 
        }

        // GET: Orders
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user's ID

            if (userId == null) // If not logged in
            {
                return Unauthorized(); //return it not being found
            }

            if (User.IsInRole("Admin")) // If the user is an Admin
            {
                // Admins see all orders with their products
                var allOrders = await _context.Order.Include(o => o.OrderProducts).ThenInclude(op => op.Products).ToListAsync();
                return View(allOrders);
            }
            else if (User.IsInRole("Producer")) // If the user is a Producer
            {
                // Get the IDs of all products this producer sells
                var producerProducts = await _context.Product.Where(p => p.Producer.UserId == userId).Select(p => p.ProductId).ToListAsync();
                // Find all orders that contain at least one of their products
                var producerOrders = await _context.OrderProduct.Where(op => producerProducts.Contains(op.ProductId)).Include(op => op.Orders).Include(op => op.Products).ToListAsync();
                // Return only distinct orders (avoid duplicates if multiple products match)
                return View(producerOrders.Select(op => op.Orders).Distinct().ToList());
            }
            else // Regular customer
            {
                // Only show orders belonging to this user
                var userOrders = await _context.Order.Where(o => o.UserId == userId).Include(o => o.OrderProducts).ThenInclude(op => op.Products).ToListAsync();
                return View(userOrders);
            }
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound(); //return it not being found
            }

            // Get all products belonging to this order
            var orders = await _context.OrderProduct
                .Where(op => op.OrderId == id)
                .Include(op => op.Orders)
                .Include(op => op.Products)
                .ToListAsync();

            if (orders == null) // If no results
            {
                return NotFound(); //return it not being found
            }

            return View(orders); // Show order details
        }

        // GET: Orders/Create
        public IActionResult Create(int basketId)
        {
            ViewBag.basketId = basketId; // Pass the basket ID to the view
            return View(); // Show the create order form
        }

        // POST: Orders/Create
        [HttpPost]
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> Create([Bind("OrderId,UserId,Subtotal,OrderType,Collection,Delivery,OrderStatus,CollectionDate,OrderDate")] Order order, int basketId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user's ID

            if (userId == null) // If not logged in
            {
                ViewBag.BasketId = basketId; // Pass basket ID back to view
                return View(order); // Show form again
            }

            // Assign values that shouldn't come from user input
            order.UserId = userId; // Set the order owner
            ModelState.Remove("UserId"); // Remove from validation since we're setting it manually

            order.OrderDate = DateOnly.FromDateTime(DateTime.Today); // Set today's date
            ModelState.Remove("OrderDate"); // Remove from validation

            order.OrderStatus = "Pending"; // New orders always start as pending
            ModelState.Remove("OrderStatus"); // Remove from validation

            // Find the user's active basket matching the given basket ID
            var basket = await _context.Basket
                .FirstOrDefaultAsync(x => x.BasketId == basketId && x.UserId == userId && x.Status);

            if (basket == null) // If no matching basket found
            {
                return NotFound(); //return it not being found
            }

            // Get all products in the basket
            var basketProducts = await _context.BasketProduct
                .Where(x => x.BasketId == basketId)
                .Include(x => x.Products)
                .ToListAsync();

            if (!basketProducts.Any()) // If the basket is empty
            {
                ModelState.AddModelError("", "Your basket is empty."); // Show error
                ViewBag.BasketId = basketId;
                return View(order); // Show form again
            }

            float subtotal = 0.00f; // Start subtotal at zero

            foreach (var basketProduct in basketProducts) // Loop through each item
            {
                var productTotal = basketProduct.Products.Price * basketProduct.Quaility; // Price × quantity
                subtotal = productTotal + subtotal; // Add to running total
            }

            // Count how many previous orders the user has placed
            var orderCount = await _context.Order.CountAsync(x => x.UserId == userId);

            float discount = 0f; // Default discount is zero

            if (orderCount >= 3) // If user has 3 or more previous orders
            {
                discount = subtotal * 10f; 
            }

            order.Subtotal = subtotal - discount; // Set the final subtotal
            ModelState.Remove("Subtotal"); // Remove from validation

            // Validate delivery/collection selection
            if (!order.Collection && !order.Delivery) // If neither option was chosen
            {
                ModelState.AddModelError("Delivery", "Must choose Collection or Delivery"); // Show error
            }

            if (order.Collection) // If collection was chosen
            {
                ModelState.Remove("OrderType"); // Delivery type not needed for collection

                if (order.CollectionDate == null) // If no collection date was given
                {
                    ModelState.AddModelError("CollectionDate", "Collection date is required"); // Show error
                }
                else
                {
                    var earliestDate = DateOnly.FromDateTime(DateTime.Today.AddDays(2)); // Minimum 2 days from now

                    if (order.CollectionDate.Value < earliestDate) // If date is too soon
                    {
                        ModelState.AddModelError("CollectionDate", "Collection must be at least 2 days from today"); // Show error
                    }
                }
            }

            if (order.Delivery) // If delivery was chosen
            {
                ModelState.Remove("CollectionDate"); // Collection date not needed for delivery

                if (string.IsNullOrWhiteSpace(order.OrderType)) // If no delivery type given
                {
                    ModelState.AddModelError("OrderType", "Delivery is required."); // Show error
                }
            }

            if (!ModelState.IsValid) // If there are any validation errors
            {
                ViewBag.BasketId = basketId;
                return View(order); // Show the form again with errors
            }

            // Save the new order to the database
            _context.Order.Add(order);
            await _context.SaveChangesAsync(); // Save to get the generated OrderId

            foreach (var basketProduct in basketProducts) // Loop through basket items
            {
                if (basketProduct.Products.stock < basketProduct.Quaility) // If not enough stock
                {
                    ModelState.AddModelError("", $"Not enough stock for{basketProduct.Products.ProductName}"); // Show error
                    ViewBag.BasketId = basketId;
                    return View(order); // Show form again
                }

                // Create an order product record for each basket item
                var orderProduct = new OrderProduct
                {
                    OrderId = order.OrderId,
                    ProductId = basketProduct.ProductId,
                    Quantity = basketProduct.Quaility // Copy quantity from basket
                };

                _context.OrderProduct.Add(orderProduct); // Add to database

                basketProduct.Products.stock -= basketProduct.Quaility; // Reduce stock by purchased quantity
            }

            basket.Status = false; // Close the basket so it's no longer active
            await _context.SaveChangesAsync(); // Save all changes

            return RedirectToAction("Index", "Orders"); // Go to orders list
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound(); //return it not being found
            }

            var order = await _context.Order.FindAsync(id); // Find the order

            if (order == null) // If not found
            {
                return NotFound();//return it not being found
            }

            return View(order); // Show the edit form
        }

        // POST: Orders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken] // Protect against CSRF
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,UserId,Subtotal,OrderType,Collection,Delivery,OrderStatus,CollectionDate,OrderDate")] Order order)
        {
            if (id != order.OrderId) // If URL ID doesn't match model ID
            {
                return NotFound(); //return it not being found
            }

            if (ModelState.IsValid) // If form data is valid
            {
                try
                {
                    _context.Update(order); // Update the order
                    await _context.SaveChangesAsync(); // Save changes
                }
                catch (DbUpdateConcurrencyException) // Handle edit conflicts
                {
                    if (!OrderExists(order.OrderId)) // If order no longer exists
                    {
                        return NotFound();//return it not being found
                    }
                    else
                    {
                        throw; 
                    }
                }
                return RedirectToAction(nameof(Index)); // Go back to list
            }
            return View(order); // If invalid, show form again
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound(); //return it not being found
            }

            // Find the order by ID
            var order = await _context.Order
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null) // If not found
            {
                return NotFound();//return it not being found
            }

            return View(order); // Show delete confirmation page
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id); // Find the order

            if (order != null) // If it exists
            {
                _context.Order.Remove(order); // Mark for absent
            }

            await _context.SaveChangesAsync(); // Commit it
            return RedirectToAction(nameof(Index)); // Return to list
        }

        //Check if an order with the given ID exists
        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderId == id);
        }
    }
}
