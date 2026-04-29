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
    public class ProducersController : Controller
    {
        private readonly ApplicationDbContext _context; // Database connection

        public ProducersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Producers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Producer.ToListAsync()); // Return all producers to the view
        }

        // GET: Producers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound();//return it not being found
            }

            // Find the producer by ID
            var producer = await _context.Producer
                .FirstOrDefaultAsync(m => m.ProducerId == id);

            if (producer == null) // If not found
            {
                return NotFound(); //return it not being found
            }

            return View(producer); // Show producer details
        }

        // GET: Producers/Create
        public IActionResult Create()
        {
            return View(); // Show the create form
        }

        // POST: Producers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProducerId,UserId,ProducerName,ProducerEmail,ProducerInformation")] Producer producer)
        {
            if (ModelState.IsValid) // If form data is valid
            {
                _context.Add(producer); // Add the producer to the database
                await _context.SaveChangesAsync(); // Save
                return RedirectToAction(nameof(Index)); // Go to the list
            }
            return View(producer); // If invalid, show form again
        }

        // GET: Producers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound(); //return it not being found
            }

            var producer = await _context.Producer.FindAsync(id); // Find by ID

            if (producer == null) // If not found
            {
                return NotFound(); //return it not being found
            }

            return View(producer); // Show the edit form
        }

        // POST: Producers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProducerId,UserId,ProducerName,ProducerEmail,ProducerInformation")] Producer producer)
        {
            if (id != producer.ProducerId) // If URL ID doesn't match model ID
            {
                return NotFound();//return it not being found
            }

            if (ModelState.IsValid) // If form data is valid
            {
                try
                {
                    _context.Update(producer); // Update the producer
                    await _context.SaveChangesAsync(); // Save changes
                }
                catch (DbUpdateConcurrencyException) // Handle edit conflicts
                {
                    if (!ProducerExists(producer.ProducerId)) // If producer no longer exists
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
            return View(producer); // If invalid, show form again
        }

        // GET: Producers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) // If no ID provided
            {
                return NotFound(); //return it not being found
            }

            // Find the producer by ID
            var producer = await _context.Producer
                .FirstOrDefaultAsync(m => m.ProducerId == id);

            if (producer == null) // If not found
            {
                return NotFound(); //return it not being found
            }

            return View(producer); // Show delete confirmation
        }

        // POST: Producers/Delete/5
        [HttpPost, ActionName("Delete")] 
        [ValidateAntiForgeryToken] 
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producer = await _context.Producer.FindAsync(id); // Find the producer

            if (producer != null) // If it exists
            {
                _context.Producer.Remove(producer); // Mark for absent
            }

            await _context.SaveChangesAsync(); // Commit it
            return RedirectToAction(nameof(Index)); // Return to list
        }

        //Check if a producer with the given ID exists
        private bool ProducerExists(int id)
        {
            return _context.Producer.Any(e => e.ProducerId == id);
        }
    }
}
