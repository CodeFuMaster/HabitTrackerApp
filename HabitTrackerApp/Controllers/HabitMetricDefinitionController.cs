using HabitTrackerApp.Data;
using HabitTrackerApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HabitTrackerApp.Controllers
{
  public class HabitMetricDefinitionController : Controller
  {
    private readonly AppDbContext _context;

    public HabitMetricDefinitionController(AppDbContext context)
    {
      _context = context;
    }

    // GET: /HabitMetricDefinition/Index?habitId=1
    // Lists all metrics for a given Habit.
    [HttpGet]
    public async Task<IActionResult> Index(int habitId)
    {
      // 1) Verify the habit exists
      var habit = await _context.Habits.FindAsync(habitId);
      if (habit == null)
        return NotFound();

      // 2) Fetch all metric definitions for this habit
      var metrics = await _context.HabitMetricDefinitions
          .Where(m => m.HabitId == habitId)
          .ToListAsync();

      // 3) Pass Habit info to the view for display
      ViewBag.HabitName = habit.Name;
      ViewBag.HabitId = habit.Id;

      return View(metrics);
    }

    // GET: /HabitMetricDefinition/Create?habitId=1
    // Show a form to create a new metric for a specific Habit.
    [HttpGet]
    public IActionResult Create(int habitId)
    {
      // Pre-fill the HabitId
      var model = new HabitMetricDefinition
      {
        HabitId = habitId
      };
      return View(model);
    }

    // POST: /HabitMetricDefinition/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HabitMetricDefinition metric)
    {
      if (!ModelState.IsValid)
      {
        // If validation fails, re-display the form
        return View(metric);
      }

      // Save the new metric definition
      _context.HabitMetricDefinitions.Add(metric);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index), new { habitId = metric.HabitId });
    }

    // GET: /HabitMetricDefinition/Edit/5
    // Show a form to edit an existing metric definition
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null) return NotFound();

      var metric = await _context.HabitMetricDefinitions.FindAsync(id);
      if (metric == null) return NotFound();

      return View(metric);
    }

    // POST: /HabitMetricDefinition/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, HabitMetricDefinition metric)
    {
      if (id != metric.Id)
        return NotFound();

      if (!ModelState.IsValid)
      {
        // Re-display form if validation fails
        return View(metric);
      }

      try
      {
        _context.Update(metric);
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        // If the metric no longer exists in DB:
        if (!_context.HabitMetricDefinitions.Any(x => x.Id == metric.Id))
          return NotFound();
        else
          throw;
      }

      return RedirectToAction(nameof(Index), new { habitId = metric.HabitId });
    }

    // GET: /HabitMetricDefinition/Delete/5
    // Confirm deletion of an existing metric
    [HttpGet]
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null) return NotFound();

      var metric = await _context.HabitMetricDefinitions
          .Include(m => m.Habit)
          .FirstOrDefaultAsync(m => m.Id == id);
      if (metric == null) return NotFound();

      return View(metric);
    }

    // POST: /HabitMetricDefinition/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var metric = await _context.HabitMetricDefinitions.FindAsync(id);
      if (metric == null) return NotFound();

      _context.HabitMetricDefinitions.Remove(metric);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index), new { habitId = metric.HabitId });
    }
  }
}
