using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTrackerApp.Data;
using HabitTrackerApp.Models;

namespace HabitTrackerApp.Api
{
    /// <summary>
    /// API Controller for mobile app synchronization - Habits
    /// Provides REST endpoints for MAUI app to sync with PostgreSQL database
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class HabitApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<HabitApiController> _logger;

        public HabitApiController(AppDbContext context, ILogger<HabitApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Get all habits (including soft-deleted for sync purposes)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Habit>>> GetHabits()
        {
            try
            {
                var habits = await _context.Habits
                    .IgnoreQueryFilters()
                    .Include(h => h.Category)
                    .OrderBy(h => h.Name)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {habits.Count} habits for sync");
                return Ok(habits);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving habits");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a single habit by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Habit>> GetHabit(int id)
        {
            try
            {
                var habit = await _context.Habits
                    .IgnoreQueryFilters()
                    .Include(h => h.Category)
                    .Include(h => h.DailyHabitEntries)
                    .FirstOrDefaultAsync(h => h.Id == id);

                if (habit == null)
                {
                    return NotFound();
                }

                return Ok(habit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving habit {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new habit
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Habit>> CreateHabit([FromBody] Habit habit)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                habit.CreatedDate = DateTimeOffset.UtcNow;
                habit.LastModifiedDate = DateTimeOffset.UtcNow;

                _context.Habits.Add(habit);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created habit: {habit.Name} (ID: {habit.Id})");
                return CreatedAtAction(nameof(GetHabit), new { id = habit.Id }, habit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating habit");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing habit
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHabit(int id, [FromBody] Habit habit)
        {
            try
            {
                if (id != habit.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var existingHabit = await _context.Habits.FindAsync(id);
                if (existingHabit == null)
                {
                    return NotFound();
                }

                // Update only the fields that exist in PostgreSQL model
                existingHabit.Name = habit.Name;
                existingHabit.Description = habit.Description;
                existingHabit.ShortDescription = habit.ShortDescription;
                existingHabit.RecurrenceType = habit.RecurrenceType;
                existingHabit.WeeklyDays = habit.WeeklyDays;
                existingHabit.MonthlyDays = habit.MonthlyDays;
                existingHabit.SpecificDate = habit.SpecificDate;
                existingHabit.TimeOfDay = habit.TimeOfDay;
                existingHabit.TimeOfDayEnd = habit.TimeOfDayEnd;
                existingHabit.CategoryId = habit.CategoryId;
                existingHabit.Tags = habit.Tags;
                existingHabit.ImageUrl = habit.ImageUrl;
                existingHabit.IsDeleted = habit.IsDeleted;
                existingHabit.LastModifiedDate = DateTimeOffset.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated habit: {habit.Name} (ID: {id})");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating habit {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a habit (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHabit(int id)
        {
            try
            {
                var habit = await _context.Habits.FindAsync(id);
                if (habit == null)
                {
                    return NotFound();
                }

                habit.IsDeleted = true;
                habit.LastModifiedDate = DateTimeOffset.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Soft deleted habit: {habit.Name} (ID: {id})");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting habit {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get daily entries for a habit
        /// </summary>
        [HttpGet("{id}/entries")]
        public async Task<ActionResult<IEnumerable<DailyHabitEntry>>> GetHabitEntries(
            int id,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.DailyHabitEntries
                    .Where(e => e.HabitId == id);

                if (startDate.HasValue)
                {
                    query = query.Where(e => e.Date >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(e => e.Date <= endDate.Value);
                }

                var entries = await query
                    .OrderByDescending(e => e.Date)
                    .ToListAsync();

                return Ok(entries);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving entries for habit {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create or update a daily habit entry
        /// </summary>
        [HttpPost("{id}/entries")]
        public async Task<ActionResult<DailyHabitEntry>> UpsertHabitEntry(int id, [FromBody] DailyHabitEntry entry)
        {
            try
            {
                if (entry.HabitId != id)
                {
                    return BadRequest("Habit ID mismatch");
                }

                var existingEntry = await _context.DailyHabitEntries
                    .FirstOrDefaultAsync(e => e.HabitId == id && e.Date.Date == entry.Date.Date);

                if (existingEntry == null)
                {
                    // Create new entry
                    entry.CheckedAt = DateTimeOffset.UtcNow;
                    entry.UpdatedAt = DateTimeOffset.UtcNow;
                    _context.DailyHabitEntries.Add(entry);
                }
                else
                {
                    // Update existing entry
                    existingEntry.IsCompleted = entry.IsCompleted;
                    existingEntry.Reflection = entry.Reflection;
                    existingEntry.Score = entry.Score;
                    existingEntry.UpdatedAt = DateTimeOffset.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Upserted habit entry for habit {id} on {entry.Date:yyyy-MM-dd}");
                return Ok(existingEntry ?? entry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error upserting entry for habit {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
