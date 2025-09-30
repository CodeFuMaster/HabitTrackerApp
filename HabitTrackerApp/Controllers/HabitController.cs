using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTrackerApp.Data;
using HabitTrackerApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using HabitTrackerApp.Models.Enums;

namespace HabitTrackerApp.Controllers
{
  // DTO for AJAX toggles
  public class ToggleRequest
  {
    public int HabitId { get; set; }
    public string Date { get; set; }
  }

  public class HabitController : Controller
  {
    private readonly AppDbContext _context;

    public HabitController(AppDbContext context)
    {
      _context = context;
    }

    // GET: Habit
    public async Task<IActionResult> Index(string filter = "active")
    {
      var habits = await _context.Habits
        .IgnoreQueryFilters()
        .Include(h => h.DailyHabitEntries)
        .Include(x => x.Category)
        .ToListAsync();

      return View(habits);
    }



    // GET: Habit/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null) return NotFound();

      var habit = await _context.Habits
          .Include(h => h.DailyHabitEntries)
          .Include(h => h.Category)
          .FirstOrDefaultAsync(m => m.Id == id);
      if (habit == null) return NotFound();

      return View(habit);
    }

    // GET: Habit/Create
    public async Task<IActionResult> Create()
    {
      // Load categories from DB
      var categories = await _context.Categories
        .OrderBy(c => c.Name)
        .ToListAsync();

      // Convert to a list of SelectListItem
      ViewBag.CategoryOptions = categories.Select(c => new SelectListItem
      {
        Value = c.Id.ToString(),
        Text = c.Name
      }).ToList();

      return View();
    }

    // POST: Habit/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Habit habit)
    {
      // --- NEW: Check if TimeOfDay < TimeOfDayEnd ---
      if (habit.TimeOfDay.HasValue && habit.TimeOfDayEnd.HasValue)
      {
        if (habit.TimeOfDay >= habit.TimeOfDayEnd)
        {
          ModelState.AddModelError("", "Start time must be earlier than end time.");
        }
      }

      // --- NEW: Overlap check ---
      if (ModelState.IsValid)
      {
        // If the new habit has valid times, ensure no overlap
        if (habit.TimeOfDay.HasValue && habit.TimeOfDayEnd.HasValue)
        {
          // Get all existing habits from the DB that also have valid times.
          var existingHabits = await _context.Habits
            .Where(h => !h.IsDeleted && h.TimeOfDay.HasValue && h.TimeOfDayEnd.HasValue)
            .ToListAsync();

          foreach (var h in existingHabits)
          {
            bool checkOverlap = false;

            // If both habits are Daily, then they always occur on the same day.
            if (habit.RecurrenceType == RecurrenceType.Daily && h.RecurrenceType == RecurrenceType.Daily)
            {
              checkOverlap = true;
            }
            // If both habits are Weekly, check if they share any common day.
            else if (habit.RecurrenceType == RecurrenceType.Weekly && h.RecurrenceType == RecurrenceType.Weekly)
            {
              if (!string.IsNullOrEmpty(habit.WeeklyDays) && !string.IsNullOrEmpty(h.WeeklyDays))
              {
                var newDays = habit.WeeklyDays.Split(',')
                                  .Select(s => s.Trim().ToLower());
                var existingDays = h.WeeklyDays.Split(',')
                                     .Select(s => s.Trim().ToLower());
                if (newDays.Intersect(existingDays).Any())
                {
                  checkOverlap = true;
                }
              }
            }
            // If both are OneTime, check if they have the same specific date.
            else if (habit.RecurrenceType == RecurrenceType.OneTime && h.RecurrenceType == RecurrenceType.OneTime)
            {
              if (habit.SpecificDate.HasValue && h.SpecificDate.HasValue &&
                  habit.SpecificDate.Value.Date == h.SpecificDate.Value.Date)
              {
                checkOverlap = true;
              }
            }
            // For mixed cases (e.g., one is Daily and one is Weekly), assume the daily habit applies to every day.
            else if (habit.RecurrenceType == RecurrenceType.Daily || h.RecurrenceType == RecurrenceType.Daily)
            {
              checkOverlap = true;
            }
            // Otherwise, if recurrence types differ (e.g., one is Weekly and the other OneTime),
            // you might decide that they don't conflict. Here, we'll only check if checkOverlap is true.

            if (checkOverlap)
            {
              // Overlap formula: [Start, End] of new habit intersects with [Start, End] of existing habit.
              if (habit.TimeOfDay < h.TimeOfDayEnd && habit.TimeOfDayEnd > h.TimeOfDay)
              {
                ModelState.AddModelError("",
                    $"Time overlaps with existing habit '{h.Name}' ({h.TimeOfDay} - {h.TimeOfDayEnd}).");
                break;
              }
            }
          }
        }
      }



      if (ModelState.IsValid)
      {
        habit.CreatedDate = DateTimeOffset.UtcNow;
        habit.LastModifiedDate = DateTimeOffset.UtcNow;

        // If we pass all validation, create the habit
        _context.Add(habit);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
      }

      // If we got here, something was invalid or overlapping
      return View(habit);
    }

    // GET: Habit/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null) return NotFound();

      var dbHabit = await _context.Habits.FindAsync(id);
      if (dbHabit == null) return NotFound();

      // Category options
      var categories = await _context.Categories
        .OrderBy(c => c.Name)
        .ToListAsync();
      ViewBag.CategoryOptions = categories.Select(c => new SelectListItem
      {
        Value = c.Id.ToString(),
        Text = c.Name
      }).ToList();

      return View(dbHabit);
    }

    // POST: Habit/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Habit postedHabit)
    {
      if (id != postedHabit.Id) return NotFound();

      // 1. Validate time order
      if (postedHabit.TimeOfDay.HasValue && postedHabit.TimeOfDayEnd.HasValue)
      {
        if (postedHabit.TimeOfDay >= postedHabit.TimeOfDayEnd)
        {
          ModelState.AddModelError("", "Start time must be earlier than end time.");
        }
      }

      // 2. If so far valid => do overlap check
      if (ModelState.IsValid && postedHabit.TimeOfDay.HasValue && postedHabit.TimeOfDayEnd.HasValue)
      {
        // Compare against other active habits only (excluding the current habit)
        var existingHabits = await _context.Habits
            .Where(h => h.Id != postedHabit.Id && !h.IsDeleted && h.TimeOfDay.HasValue && h.TimeOfDayEnd.HasValue)
            .AsNoTracking()
            .ToListAsync();

        foreach (var h in existingHabits)
        {
          bool checkOverlap = false;

          // If both are Daily, always check
          if (postedHabit.RecurrenceType == RecurrenceType.Daily && h.RecurrenceType == RecurrenceType.Daily)
          {
            checkOverlap = true;
          }
          // If both are Weekly, check if they share any common day
          else if (postedHabit.RecurrenceType == RecurrenceType.Weekly && h.RecurrenceType == RecurrenceType.Weekly)
          {
            if (!string.IsNullOrEmpty(postedHabit.WeeklyDays) && !string.IsNullOrEmpty(h.WeeklyDays))
            {
              var newDays = postedHabit.WeeklyDays.Split(',')
                                 .Select(s => s.Trim().ToLower());
              var existingDays = h.WeeklyDays.Split(',')
                                   .Select(s => s.Trim().ToLower());
              if (newDays.Intersect(existingDays).Any())
              {
                checkOverlap = true;
              }
            }
          }
          // If both are OneTime, check if they have the same specific date.
          else if (postedHabit.RecurrenceType == RecurrenceType.OneTime && h.RecurrenceType == RecurrenceType.OneTime)
          {
            if (postedHabit.SpecificDate.HasValue && h.SpecificDate.HasValue &&
                postedHabit.SpecificDate.Value.Date == h.SpecificDate.Value.Date)
            {
              checkOverlap = true;
            }
          }
          // For mixed cases, if one habit is Daily, assume it applies every day.
          else if (postedHabit.RecurrenceType == RecurrenceType.Daily || h.RecurrenceType == RecurrenceType.Daily)
          {
            checkOverlap = true;
          }

          if (checkOverlap)
          {
            // Check if the time intervals overlap
            if (postedHabit.TimeOfDay < h.TimeOfDayEnd && postedHabit.TimeOfDayEnd > h.TimeOfDay)
            {
              ModelState.AddModelError("",
                $"Time overlaps with active habit '{h.Name}' ({h.TimeOfDay} - {h.TimeOfDayEnd}).");
              break;
            }
          }
        }
      }


      // 3. If we have errors => return view with them displayed
      if (!ModelState.IsValid)
      {
        return View(postedHabit);
      }

      // 4. Load the existing habit from DB (tracked by EF)
      var dbHabit = await _context.Habits
        .Include(h => h.DailyHabitEntries)
        .FirstOrDefaultAsync(h => h.Id == id);

      if (dbHabit == null) return NotFound();

      bool hasEntries = dbHabit.DailyHabitEntries.Any();
      if (hasEntries)
      {
        // If user tried to change name
        if (postedHabit.Name != dbHabit.Name)
        {
          ModelState.AddModelError("", "Cannot rename habit once daily entries exist. Create a new habit instead.");
        }
        // If user tried to change time
        if (postedHabit.TimeOfDay != dbHabit.TimeOfDay ||
            postedHabit.TimeOfDayEnd != dbHabit.TimeOfDayEnd)
        {
          ModelState.AddModelError("", "Cannot change times once daily entries exist. Create a new habit instead.");
        }

        if (!ModelState.IsValid)
        {
          // Re-show the form with these errors
          return View(postedHabit);
        }
      }

      // 5. Map postedHabit’s properties onto dbHabit
      dbHabit.Name = postedHabit.Name;
      dbHabit.Description = postedHabit.Description;
      dbHabit.TimeOfDay = postedHabit.TimeOfDay;
      dbHabit.TimeOfDayEnd = postedHabit.TimeOfDayEnd;
      dbHabit.RecurrenceType = postedHabit.RecurrenceType;
      dbHabit.WeeklyDays = postedHabit.WeeklyDays;
      dbHabit.MonthlyDays = postedHabit.MonthlyDays;
      dbHabit.SpecificDate = postedHabit.SpecificDate;
      dbHabit.Category = postedHabit.Category;
      dbHabit.CategoryId = postedHabit.CategoryId;
      dbHabit.Tags = postedHabit.Tags;
      dbHabit.ShortDescription = postedHabit.ShortDescription;
      dbHabit.ImageUrl = postedHabit.ImageUrl;
      dbHabit.IsDeleted = !postedHabit.IsDeleted;
      // Update the LastModifiedDate to now (leave CreatedDate unchanged)
      dbHabit.LastModifiedDate = DateTimeOffset.UtcNow;
      // ... map any other relevant fields

      // 6. Save changes — no _context.Update(...) needed, because dbHabit is tracked
      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!_context.Habits.Any(e => e.Id == postedHabit.Id))
          return NotFound();
        else
          throw;
      }

      return RedirectToAction(nameof(Index));
    }


    // GET: Habit/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null) return NotFound();

      var habit = await _context.Habits
          .FirstOrDefaultAsync(m => m.Id == id);
      if (habit == null) return NotFound();

      return View(habit);
    }

    // POST: Habit/Soft Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var habit = await _context.Habits.FindAsync(id);
      if (habit == null) return NotFound();

      habit.IsDeleted = true;

      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }

    [HttpPost, ActionName("HardDelete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> HardDeleteConfirmed(int id)
    {
      var habit = await _context.Habits.FindAsync(id);
      if (habit == null)
      {
        return NotFound();
      }
      _context.Habits.Remove(habit); // Hard deletion from DB
      await _context.SaveChangesAsync();
      return RedirectToAction(nameof(Index));
    }

    // DayView
    public async Task<IActionResult> DayView(string date)
    {
      if (!DateTime.TryParse(date, out var selectedDate))
        selectedDate = DateTime.Today;

      var habits = await _context.Habits
        .Where(h => !h.IsDeleted)
          .Include(h => h.DailyHabitEntries)
          .Include(x => x.Category)
          .ToListAsync();

      var habitsForDay = habits.Where(h => h.OccursOn(selectedDate)).ToList();
      ViewBag.SelectedDate = selectedDate;
      return View(habitsForDay);
    }

    // WeeklyView
    [HttpGet]
    public async Task<IActionResult> WeeklyView(string weekStart)
    {
      if (!DateTime.TryParse(weekStart, out var weekStartDate))
      {
        var today = DateTime.Today;
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        weekStartDate = today.AddDays(-diff);
      }
      var habits = await _context.Habits
          .Where(h => !h.IsDeleted)  // exclude deleted
          .Include(h => h.DailyHabitEntries)
          .ToListAsync();

      ViewBag.WeekStart = weekStartDate;
      return View(habits);
    }

    // WeeklyTableView
    [HttpGet]
    public async Task<IActionResult> WeeklyTableView(string weekStart)
    {
      // 1) Determine the Monday of the requested week, or use this week’s Monday if invalid.
      DateTime mondayOfWeek;
      if (!string.IsNullOrEmpty(weekStart) && Regex.IsMatch(weekStart, @"^\d{4}-W\d{2}$"))
      {
        var splitted = weekStart.Split("-W");
        if (int.TryParse(splitted[0], out int year) && int.TryParse(splitted[1], out int week))
        {
          mondayOfWeek = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
        }
        else
        {
          mondayOfWeek = GetMondayOfCurrentWeek();
        }
      }
      else
      {
        mondayOfWeek = GetMondayOfCurrentWeek();
      }

      // 2) Build a list of the 7 days in the selected week.
      var weekDays = Enumerable.Range(0, 7)
        .Select(i => mondayOfWeek.AddDays(i).Date)
        .ToList();

      // 3) Query the database for all non-deleted habits (plus their entries, etc.).
      var allHabits = await _context.Habits
        .Where(h => !h.IsDeleted)  // exclude deleted
        .Include(h => h.DailyHabitEntries)
        .Include(h => h.Category)
        .OrderBy(h => h.TimeOfDay ?? TimeSpan.Zero)
        .ToListAsync();

      // 4) Filter out habits that do *not* occur on any day of this week 
      //    (i.e. if OccursOn(...) is false for all 7 days, exclude that habit).
      var filteredHabits = allHabits
        .Where(h => weekDays.Any(day => h.OccursOn(day)))
        .ToList();

      // 5) Return only the habits that actually need attention this week
      ViewBag.WeekStart = mondayOfWeek;
      return View(filteredHabits);
    }


    private DateTime GetMondayOfCurrentWeek()
    {
      var today = DateTime.Today;
      int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
      return today.AddDays(-diff);
    }

    // Reflection
    [HttpGet]
    public async Task<IActionResult> AddReflection(int habitId, string date, string returnUrl)
    {
      // 1. Parse the date or default to today
      if (!DateTime.TryParse(date, out var parsedDate))
        parsedDate = DateTime.Today;

      // 2. Load (or not) the existing daily entry
      var entry = await _context.DailyHabitEntries
          .FirstOrDefaultAsync(e => e.HabitId == habitId && e.Date == parsedDate);

      // 3. Build a ReflectionViewModel
      var viewModel = new ReflectionViewModel
      {
        HabitId = habitId,
        Date = parsedDate,
        Reflection = entry?.Reflection,
        Score = entry?.Score,
        ReturnUrl = returnUrl
      };

      // 4. (Optional) If using custom metrics, load them:
      //    - get all HabitMetricDefinition for this habit
      var metricDefs = await _context.HabitMetricDefinitions
          .Where(m => m.HabitId == habitId)
          .ToListAsync();

      // 5. Load existing DailyMetricValue for each metric on this date
      //    to pre-fill user values if they exist
      List<DailyMetricValue> dailyValues = new List<DailyMetricValue>();
      if (entry != null)
      {
        dailyValues = await _context.DailyMetricValues
            .Where(d => d.DailyHabitEntryId == entry.Id)
            .ToListAsync();
      }

      // 6. Convert each metric definition to MetricInputDto, merging any daily values
      foreach (var def in metricDefs)
      {
        var dto = new MetricInputDto
        {
          MetricDefinitionId = def.Id,
          Name = def.Name,
          Unit = def.Unit,
          DataType = def.DataType
        };

        // If there's an existing daily value, fill it
        var existingVal = dailyValues
            .FirstOrDefault(d => d.HabitMetricDefinitionId == def.Id);
        if (existingVal != null)
        {
          // If numeric data type, load existingVal.NumericValue
          // If text, load existingVal.TextValue, etc.
          switch (def.DataType)
          {
            case MetricDataType.Numeric:
              dto.UserNumericValue = existingVal.NumericValue;
              break;
            case MetricDataType.Text:
              dto.UserTextValue = existingVal.TextValue;
              break;
            case MetricDataType.Boolean:
              // We can treat "1" as true, 0 as false
              dto.UserBoolValue = existingVal.BooleanValue;
              break;
          }
        }

        viewModel.Metrics.Add(dto);
      }

      // 7. Load previous day's metrics to pre-fill empty fields.
      // Calculate previous day.
      var previousDate = parsedDate.AddDays(-1);
      // Load previous day's entry including its DailyMetricValues and associated HabitMetricDefinition
      var previousEntry = await _context.DailyHabitEntries
        .Include(e => e.DailyMetricValues)
        .ThenInclude(d => d.HabitMetricDefinition)
        .FirstOrDefaultAsync(e => e.HabitId == habitId && e.Date == previousDate);

      // Build a dictionary mapping metric definition IDs to their stored values
      var previousMetrics = new Dictionary<int, object>();
      if (previousEntry != null && previousEntry.DailyMetricValues != null)
      {
        foreach (var dm in previousEntry.DailyMetricValues)
        {
          if (dm.HabitMetricDefinitionId.HasValue)
          {
            // You can include both numeric and text values
            previousMetrics[dm.HabitMetricDefinitionId.Value] = new
            {
              UserNumericValue = dm.NumericValue,
              UserTextValue = dm.TextValue
            };
          }
        }
      }
      // Serialize to JSON and store in ViewBag (empty dictionary if none)
      ViewBag.PreviousMetricsJson = JsonSerializer.Serialize(previousMetrics);

      return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddReflection(ReflectionViewModel model)
    {
      // 1. Basic validation
      if (!ModelState.IsValid)
      {
        foreach (var kvp in ModelState)
        {
          var fieldKey = kvp.Key;
          var errors = kvp.Value.Errors;
          foreach (var err in errors)
          {
            Console.WriteLine($"{fieldKey}: {err.ErrorMessage}");
          }
        }
        return View(model);
      }

      // Ensure model.Metrics is not null
      if (model.Metrics == null)
      {
        model.Metrics = new List<MetricInputDto>();
      }

      // 2. Find or create the DailyHabitEntry
      var entry = await _context.DailyHabitEntries
          .FirstOrDefaultAsync(e => e.HabitId == model.HabitId && e.Date == model.Date);

      if (entry == null)
      {
        entry = new DailyHabitEntry
        {
          HabitId = model.HabitId,
          Date = model.Date,
          IsCompleted = false,
          Reflection = model.Reflection,
          Score = model.Score
        };
        _context.DailyHabitEntries.Add(entry);
      }
      else
      {
        entry.Reflection = model.Reflection;
        entry.Score = model.Score;
        _context.DailyHabitEntries.Update(entry);
      }
      await _context.SaveChangesAsync(); // Save so entry.Id is available

      // 3. Handle custom metric values
      var dailyValList = await _context.DailyMetricValues
        .Where(v => v.DailyHabitEntryId == entry.Id).Include(dailyMetricValue => dailyMetricValue.HabitMetricDefinition)
        .ToListAsync();

      foreach (var dto in model.Metrics)
      {
        if (dto == null)
          continue;

        // Find or create DailyMetricValue for this metric
        var dailyVal = dailyValList.FirstOrDefault(v => v.HabitMetricDefinitionId == dto.MetricDefinitionId);
        if (dailyVal == null)
        {
          dailyVal = new DailyMetricValue
          {
            DailyHabitEntryId = entry.Id
          };
          _context.DailyMetricValues.Add(dailyVal);
        }


        // If this is a new (ephemeral) metric, set its properties on the DailyMetricValue
        if (dto.MetricDefinitionId == 0)
        {
    // Instantiate a new HabitMetricDefinition with the proper HabitId.
    if (dailyVal.HabitMetricDefinition == null)
    {
        dailyVal.HabitMetricDefinition = new HabitMetricDefinition
        {
            HabitId = model.HabitId, // Set the foreign key from the parent habit
            Name = dto.Name,
            Unit = dto.Unit,
            DataType = dto.DataType
        };
    }
    else
    {
        dailyVal.HabitMetricDefinition.Name = dto.Name;
        dailyVal.HabitMetricDefinition.Unit = dto.Unit;
        dailyVal.HabitMetricDefinition.DataType = dto.DataType;
    }
    dailyVal.HabitMetricDefinitionId = dailyVal.HabitMetricDefinition.Id; // or leave it null if the relationship is set to cascade
}
        else
        {
          dailyVal.HabitMetricDefinitionId = dto.MetricDefinitionId;
        }

        // Assign user input based on the data type
        switch (dto.DataType)
        {
          case MetricDataType.Numeric:
            dailyVal.NumericValue = dto.UserNumericValue;
            dailyVal.TextValue = null;
            break;
          case MetricDataType.Text:
            dailyVal.TextValue = dto.UserTextValue;
            dailyVal.NumericValue = null;
            break;
          case MetricDataType.Boolean:
            dailyVal.NumericValue = (dto.UserNumericValue.HasValue && dto.UserNumericValue.Value > 0) ? 1 : 0;
            dailyVal.TextValue = null;
            break;
        }
      }

      await _context.SaveChangesAsync();

      // 4. Redirect to ReturnUrl or fallback to DayView
      if (!string.IsNullOrEmpty(model.ReturnUrl))
        return Redirect(model.ReturnUrl);

      return RedirectToAction("DayView", new { date = model.Date.ToString("yyyy-MM-dd") });
    }




    [HttpPost]
    public async Task<IActionResult> RemoveReflection(ReflectionViewModel model)
    {
      var entry = await _context.DailyHabitEntries
        .FirstOrDefaultAsync(e => e.HabitId == model.HabitId && e.Date == model.Date);

      if (entry != null)
      {
        entry.Reflection = null;
        entry.Score = null;
        // if you also want to remove daily metrics:
        // var dailyVals = _context.DailyMetricValues
        //     .Where(v => v.DailyHabitEntryId == entry.Id);
        // _context.DailyMetricValues.RemoveRange(dailyVals);

        await _context.SaveChangesAsync();
      }

      if (!string.IsNullOrEmpty(model.ReturnUrl))
        return Redirect(model.ReturnUrl);

      return RedirectToAction("WeeklyTableView");
    }


    // This endpoint will handle toggling a habit's completion status without reloading the page.
    [HttpPost]
    [IgnoreAntiforgeryToken] // or handle with token if you prefer
    public async Task<IActionResult> ToggleCompletedAjax([FromBody] ToggleRequest model)
    {
      // 1. Parse the date
      if (!DateTime.TryParse(model.Date, out var selectedDate))
        selectedDate = DateTime.Today;

      // 2. Find or create the entry
      var entry = await _context.DailyHabitEntries
          .FirstOrDefaultAsync(e => e.HabitId == model.HabitId && e.Date == selectedDate.Date);

      if (entry == null)
      {
        entry = new DailyHabitEntry
        {
          HabitId = model.HabitId,
          Date = selectedDate.Date,
          IsCompleted = true,
          CheckedAt = DateTimeOffset.UtcNow,
          UpdatedAt = DateTimeOffset.UtcNow
        };
        _context.DailyHabitEntries.Add(entry);
      }
      else
      {
        entry.IsCompleted = !entry.IsCompleted;
        entry.UpdatedAt =DateTimeOffset.UtcNow;
        _context.DailyHabitEntries.Update(entry);
      }
      await _context.SaveChangesAsync();

      // Return JSON so the client can update the UI without reload
      return Json(new
      {
        success = true,
        isCompleted = entry.IsCompleted,
        message = "Toggled completion via AJAX"
      });
    }

    private bool HabitExists(int id)
    {
      return _context.Habits.Any(e => e.Id == id);
    }
  }
}
