using HabitTrackerApp.Data;
using HabitTrackerApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace HabitTrackerApp.Controllers
{
  public class CategoryController : Controller
  {
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
      _context = context;
    }

    // GET: Category
    public async Task<IActionResult> Index()
    {
      // Fetch all categories from the DB
      var categories = await _context.Categories
          .OrderBy(c => c.Name)
          .ToListAsync();
      return View(categories);
    }

    // GET: Category/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null) return NotFound();

      var category = await _context.Categories
          .FirstOrDefaultAsync(m => m.Id == id);
      if (category == null) return NotFound();

      return View(category);
    }

    // GET: Category/Create
    public IActionResult Create()
    {
      return View();
    }

    // POST: Category/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Category category)
    {
      if (!ModelState.IsValid)
      {
        // If there are validation errors, re-display the form
        return View(category);
      }

      // If valid, save new category to DB
      _context.Add(category);
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    // GET: Category/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null) return NotFound();

      var category = await _context.Categories.FindAsync(id);
      if (category == null) return NotFound();

      return View(category);
    }

    // POST: Category/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Category category)
    {
      if (id != category.Id) return NotFound();

      if (!ModelState.IsValid)
      {
        return View(category);
      }

      try
      {
        // Update the category
        _context.Update(category);
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!CategoryExists(category.Id))
          return NotFound();
        else
          throw;
      }

      return RedirectToAction(nameof(Index));
    }

    // GET: Category/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null) return NotFound();

      var category = await _context.Categories
          .FirstOrDefaultAsync(m => m.Id == id);
      if (category == null) return NotFound();

      return View(category);
    }

    // POST: Category/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var category = await _context.Categories.FindAsync(id);
      if (category != null)
      {
        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
      }
      return RedirectToAction(nameof(Index));
    }

    private bool CategoryExists(int id)
    {
      return _context.Categories.Any(e => e.Id == id);
    }
  }
}
