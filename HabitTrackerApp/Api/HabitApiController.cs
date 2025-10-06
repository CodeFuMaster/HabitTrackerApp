using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTrackerApp.Data;
using HabitTrackerApp.Models;
using HabitTrackerApp.ViewModels;

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

        private static readonly string[] DayNames = Enum.GetNames(typeof(DayOfWeek));

        private static HabitDto MapHabitToDto(Habit habit)
        {
            return new HabitDto
            {
                Id = habit.Id,
                Name = habit.Name,
                Description = habit.Description,
                ShortDescription = habit.ShortDescription,
                HabitType = habit.HabitType,
                RecurrenceType = habit.RecurrenceType,
                RecurrenceInterval = habit.RecurrenceInterval,
                WeeklyDays = habit.WeeklyDays,
                MonthlyDays = habit.MonthlyDays,
                SpecificDaysOfWeek = WeeklyDaysToNumbers(habit.WeeklyDays),
                SpecificDaysOfMonth = MonthlyDaysToNumbers(habit.MonthlyDays),
                SpecificDate = habit.SpecificDate,
                TimeOfDay = FormatTime(habit.TimeOfDay),
                TimeOfDayEnd = FormatTime(habit.TimeOfDayEnd),
                DurationMinutes = habit.DurationMinutes,
                CategoryId = habit.CategoryId,
                Tags = habit.Tags,
                ImageUrl = habit.ImageUrl,
                Color = habit.Color,
                Icon = habit.Icon,
                ReminderEnabled = habit.ReminderEnabled,
                ReminderTime = FormatTime(habit.ReminderTime),
                IsActive = !habit.IsDeleted,
                IsDeleted = habit.IsDeleted,
                CreatedDate = habit.CreatedDate,
                LastModifiedDate = habit.LastModifiedDate,
                DeviceId = habit.DeviceId,
                SyncStatus = habit.SyncStatus
            };
        }

        private static void ApplyDtoToHabit(Habit habit, HabitDto dto, bool isNew)
        {
            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                habit.Name = dto.Name;
            }
            habit.Description = NormalizeOptionalString(dto.Description);
            habit.ShortDescription = NormalizeOptionalString(dto.ShortDescription);
            habit.HabitType = dto.HabitType;
            habit.RecurrenceType = dto.RecurrenceType;
            habit.RecurrenceInterval = dto.RecurrenceInterval.HasValue && dto.RecurrenceInterval.Value > 0
                ? dto.RecurrenceInterval
                : null;
            habit.WeeklyDays = NormalizeOptionalString(dto.WeeklyDays) ?? NumbersToWeeklyDaysString(dto.SpecificDaysOfWeek);
            habit.MonthlyDays = NormalizeOptionalString(dto.MonthlyDays) ?? NumbersToMonthlyDaysString(dto.SpecificDaysOfMonth);
            habit.SpecificDate = dto.SpecificDate;
            habit.TimeOfDay = ParseTimeString(dto.TimeOfDay);
            habit.TimeOfDayEnd = ParseTimeString(dto.TimeOfDayEnd);
            habit.DurationMinutes = dto.DurationMinutes.HasValue && dto.DurationMinutes.Value > 0
                ? dto.DurationMinutes
                : null;
            habit.CategoryId = dto.CategoryId.HasValue && dto.CategoryId.Value > 0
                ? dto.CategoryId.Value
                : null;
            habit.Tags = NormalizeOptionalString(dto.Tags);
            habit.ImageUrl = NormalizeOptionalString(dto.ImageUrl);
            habit.Color = NormalizeOptionalString(dto.Color);
            habit.Icon = NormalizeOptionalString(dto.Icon);
            habit.ReminderEnabled = dto.ReminderEnabled;
            habit.ReminderTime = ParseTimeString(dto.ReminderTime);

            var isDeleted = dto.IsDeleted;
            if (!isDeleted && dto.IsActive == false)
            {
                isDeleted = true;
            }
            habit.IsDeleted = isDeleted;

            habit.DeviceId = NormalizeOptionalString(dto.DeviceId);
            habit.SyncStatus = NormalizeOptionalString(dto.SyncStatus);

            if (dto.CreatedDate.HasValue)
            {
                habit.CreatedDate = dto.CreatedDate.Value;
            }
            else if (isNew && habit.CreatedDate == default)
            {
                habit.CreatedDate = DateTimeOffset.UtcNow;
            }

            habit.LastModifiedDate = dto.LastModifiedDate ?? DateTimeOffset.UtcNow;
        }

        private static string? FormatTime(TimeSpan? value)
        {
            return value?.ToString("hh\\:mm", CultureInfo.InvariantCulture);
        }

        private static TimeSpan? ParseTimeString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (TimeSpan.TryParseExact(value, "hh\\:mm", CultureInfo.InvariantCulture, out var exact))
            {
                return exact;
            }

            if (TimeSpan.TryParseExact(value, "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out var withSeconds))
            {
                return withSeconds;
            }

            if (TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var fallback))
            {
                return fallback;
            }

            return null;
        }

        private static int[]? WeeklyDaysToNumbers(string? weeklyDays)
        {
            if (string.IsNullOrWhiteSpace(weeklyDays))
            {
                return null;
            }

            var indices = weeklyDays
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(day => day.Trim())
                .Where(day => day.Length > 0)
                .Select(day => Array.FindIndex(DayNames, name => string.Equals(name, day, StringComparison.OrdinalIgnoreCase)))
                .Where(index => index >= 0)
                .Distinct()
                .OrderBy(index => index)
                .ToArray();

            return indices.Length > 0 ? indices : null;
        }

        private static int[]? MonthlyDaysToNumbers(string? monthlyDays)
        {
            if (string.IsNullOrWhiteSpace(monthlyDays))
            {
                return null;
            }

            var values = monthlyDays
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(day => day.Trim())
                .Select(day => int.TryParse(day, out var number) ? number : (int?)null)
                .Where(number => number.HasValue)
                .Select(number => number!.Value)
                .Where(number => number >= 1 && number <= 31)
                .Distinct()
                .OrderBy(number => number)
                .ToArray();

            return values.Length > 0 ? values : null;
        }

        private static string? NumbersToWeeklyDaysString(IEnumerable<int>? days)
        {
            if (days == null)
            {
                return null;
            }

            var ordered = days
                .Where(day => day >= 0 && day < DayNames.Length)
                .Distinct()
                .OrderBy(day => day)
                .Select(day => DayNames[day])
                .ToArray();

            return ordered.Length > 0 ? string.Join(',', ordered) : null;
        }

        private static string? NumbersToMonthlyDaysString(IEnumerable<int>? days)
        {
            if (days == null)
            {
                return null;
            }

            var ordered = days
                .Where(day => day >= 1 && day <= 31)
                .Distinct()
                .OrderBy(day => day)
                .ToArray();

            return ordered.Length > 0 ? string.Join(',', ordered) : null;
        }

        private static string? NormalizeOptionalString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
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
        public async Task<ActionResult<IEnumerable<HabitDto>>> GetHabits()
        {
            try
            {
                var habits = await _context.Habits
                    .IgnoreQueryFilters()
                    .Include(h => h.Category)
                    .OrderBy(h => h.Name)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {habits.Count} habits for sync");
                var results = habits.Select(MapHabitToDto).ToList();
                return Ok(results);
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
        public async Task<ActionResult<HabitDto>> GetHabit(int id)
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

                return Ok(MapHabitToDto(habit));
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
        public async Task<ActionResult<HabitDto>> CreateHabit([FromBody] HabitDto habitDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(habitDto.Name))
                {
                    ModelState.AddModelError(nameof(habitDto.Name), "Name is required.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var habit = new Habit();
                ApplyDtoToHabit(habit, habitDto, isNew: true);

                if (habit.CreatedDate == default)
                {
                    habit.CreatedDate = DateTimeOffset.UtcNow;
                }

                if (habit.LastModifiedDate == default)
                {
                    habit.LastModifiedDate = DateTimeOffset.UtcNow;
                }

                _context.Habits.Add(habit);
                await _context.SaveChangesAsync();

                var result = MapHabitToDto(habit);
                _logger.LogInformation($"Created habit: {result.Name} (ID: {result.Id})");
                return CreatedAtAction(nameof(GetHabit), new { id = result.Id }, result);
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
        public async Task<ActionResult<HabitDto>> UpdateHabit(int id, [FromBody] HabitDto habitDto)
        {
            try
            {
                if (habitDto == null)
                {
                    return BadRequest("Habit payload is required");
                }

                if (habitDto.Id != 0 && id != habitDto.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (string.IsNullOrWhiteSpace(habitDto.Name))
                {
                    ModelState.AddModelError(nameof(habitDto.Name), "Name is required.");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingHabit = await _context.Habits.FindAsync(id);
                if (existingHabit == null)
                {
                    return NotFound();
                }

                ApplyDtoToHabit(existingHabit, habitDto, isNew: false);
                if (existingHabit.LastModifiedDate == default)
                {
                    existingHabit.LastModifiedDate = DateTimeOffset.UtcNow;
                }

                await _context.SaveChangesAsync();

                var result = MapHabitToDto(existingHabit);
                _logger.LogInformation($"Updated habit: {result.Name} (ID: {id})");
                return Ok(result);
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
