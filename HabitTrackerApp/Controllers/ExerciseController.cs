using HabitTrackerApp.Data;
using HabitTrackerApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HabitTrackerApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExerciseController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ExerciseController> _logger;

        public ExerciseController(AppDbContext context, ILogger<ExerciseController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/Exercise/seed-data
        [HttpPost("seed-data")]
        public async Task<ActionResult> SeedExerciseData()
        {
            try
            {
                _logger.LogInformation("Seeding exercise data...");
                await Data.ExerciseSeedData.SeedExercisesAsync(_context);
                
                var exerciseCount = await _context.Exercises.CountAsync();
                var habitCount = await _context.Habits.Where(h => h.HabitType != 0).CountAsync();
                
                return Ok(new
                {
                    message = "Exercise data seeded successfully",
                    totalExercises = exerciseCount,
                    habitsWithExercises = habitCount,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding exercise data");
                return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
            }
        }

        // GET: api/Exercise/habits/{habitId}/exercises
        [HttpGet("habits/{habitId}/exercises")]
        public async Task<ActionResult<List<Exercise>>> GetExercisesForHabit(int habitId)
        {
            try
            {
                var exercises = await _context.Exercises
                    .Where(e => e.HabitId == habitId && e.IsActive)
                    .OrderBy(e => e.OrderIndex)
                    .ToListAsync();

                return Ok(exercises);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exercises for habit {HabitId}", habitId);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Exercise/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Exercise>> GetExercise(int id)
        {
            try
            {
                var exercise = await _context.Exercises.FindAsync(id);

                if (exercise == null)
                {
                    return NotFound();
                }

                return Ok(exercise);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exercise {ExerciseId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Exercise
        [HttpPost]
        public async Task<ActionResult<Exercise>> CreateExercise([FromBody] Exercise exercise)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                exercise.CreatedDate = DateTime.UtcNow;
                exercise.IsActive = true;

                _context.Exercises.Add(exercise);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created exercise {ExerciseId} for habit {HabitId}", exercise.Id, exercise.HabitId);

                return CreatedAtAction(nameof(GetExercise), new { id = exercise.Id }, exercise);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exercise");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Exercise/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateExercise(int id, [FromBody] Exercise exercise)
        {
            try
            {
                if (id != exercise.Id)
                {
                    return BadRequest("ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existing = await _context.Exercises.FindAsync(id);
                if (existing == null)
                {
                    return NotFound();
                }

                // Update fields
                existing.Name = exercise.Name;
                existing.Description = exercise.Description;
                existing.OrderIndex = exercise.OrderIndex;
                existing.ImageUrl = exercise.ImageUrl;
                existing.VideoUrl = exercise.VideoUrl;
                existing.LocalVideoPath = exercise.LocalVideoPath;
                existing.DocumentUrls = exercise.DocumentUrls;
                existing.TargetSets = exercise.TargetSets;
                existing.TargetReps = exercise.TargetReps;
                existing.TargetWeight = exercise.TargetWeight;
                existing.TargetDuration = exercise.TargetDuration;
                existing.TargetRPE = exercise.TargetRPE;
                existing.RestSeconds = exercise.RestSeconds;
                existing.ExerciseType = exercise.ExerciseType;
                existing.MuscleGroups = exercise.MuscleGroups;
                existing.Equipment = exercise.Equipment;
                existing.Notes = exercise.Notes;
                existing.LastModifiedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated exercise {ExerciseId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exercise {ExerciseId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Exercise/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            try
            {
                var exercise = await _context.Exercises.FindAsync(id);
                if (exercise == null)
                {
                    return NotFound();
                }

                // Soft delete
                exercise.IsActive = false;
                exercise.LastModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Soft deleted exercise {ExerciseId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting exercise {ExerciseId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Exercise/batch
        [HttpPost("batch")]
        public async Task<ActionResult<List<Exercise>>> BatchCreateExercises([FromBody] List<Exercise> exercises)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                foreach (var exercise in exercises)
                {
                    exercise.CreatedDate = DateTime.UtcNow;
                    exercise.IsActive = true;
                    _context.Exercises.Add(exercise);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Batch created {Count} exercises", exercises.Count);

                return CreatedAtAction(nameof(GetExercisesForHabit), 
                    new { habitId = exercises.First().HabitId }, exercises);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch creating exercises");
                return StatusCode(500, "Internal server error");
            }
        }

        // PUT: api/Exercise/reorder
        [HttpPut("reorder")]
        public async Task<IActionResult> ReorderExercises([FromBody] List<ExerciseOrderUpdate> updates)
        {
            try
            {
                foreach (var update in updates)
                {
                    var exercise = await _context.Exercises.FindAsync(update.ExerciseId);
                    if (exercise != null)
                    {
                        exercise.OrderIndex = update.NewOrderIndex;
                        exercise.LastModifiedDate = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Reordered {Count} exercises", updates.Count);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering exercises");
                return StatusCode(500, "Internal server error");
            }
        }

        // ==================== EXERCISE LOGS ====================

        // POST: api/Exercise/logs
        [HttpPost("logs")]
        public async Task<ActionResult<ExerciseLog>> LogExercise([FromBody] ExerciseLog log)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (log.CompletedAt == null)
                {
                    log.CompletedAt = DateTime.UtcNow;
                }

                _context.ExerciseLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Logged exercise {ExerciseId} set {SetNumber}", 
                    log.ExerciseId, log.SetNumber);

                return CreatedAtAction(nameof(GetExerciseLogs), 
                    new { exerciseId = log.ExerciseId }, log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging exercise");
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Exercise/{exerciseId}/logs
        [HttpGet("{exerciseId}/logs")]
        public async Task<ActionResult<List<ExerciseLog>>> GetExerciseLogs(
            int exerciseId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = _context.ExerciseLogs
                    .Where(l => l.ExerciseId == exerciseId);

                if (startDate.HasValue)
                {
                    query = query.Where(l => l.Date >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(l => l.Date <= endDate.Value);
                }

                var logs = await query
                    .OrderBy(l => l.Date)
                    .ThenBy(l => l.SetNumber)
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching exercise logs for {ExerciseId}", exerciseId);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Exercise/entries/{entryId}/logs
        [HttpGet("entries/{entryId}/logs")]
        public async Task<ActionResult<List<ExerciseLog>>> GetLogsForEntry(int entryId)
        {
            try
            {
                var logs = await _context.ExerciseLogs
                    .Where(l => l.DailyHabitEntryId == entryId)
                    .OrderBy(l => l.ExerciseId)
                    .ThenBy(l => l.SetNumber)
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching logs for entry {EntryId}", entryId);
                return StatusCode(500, "Internal server error");
            }
        }

        // POST: api/Exercise/logs/batch
        [HttpPost("logs/batch")]
        public async Task<ActionResult<List<ExerciseLog>>> BatchLogExercises([FromBody] List<ExerciseLog> logs)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                foreach (var log in logs)
                {
                    if (log.CompletedAt == null)
                    {
                        log.CompletedAt = DateTime.UtcNow;
                    }
                    _context.ExerciseLogs.Add(log);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Batch logged {Count} exercise sets", logs.Count);

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error batch logging exercises");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Exercise/logs/{id}
        [HttpDelete("logs/{id}")]
        public async Task<IActionResult> DeleteExerciseLog(int id)
        {
            try
            {
                var log = await _context.ExerciseLogs.FindAsync(id);
                if (log == null)
                {
                    return NotFound();
                }

                _context.ExerciseLogs.Remove(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted exercise log {LogId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting exercise log {LogId}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Exercise/{exerciseId}/last-performance
        [HttpGet("{exerciseId}/last-performance")]
        public async Task<ActionResult<List<ExerciseLog>>> GetLastPerformance(
            int exerciseId,
            [FromQuery] DateTime? beforeDate = null)
        {
            try
            {
                var cutoffDate = beforeDate ?? DateTime.UtcNow;

                // Get the most recent date before cutoff
                var lastDate = await _context.ExerciseLogs
                    .Where(l => l.ExerciseId == exerciseId && l.Date < cutoffDate)
                    .OrderByDescending(l => l.Date)
                    .Select(l => l.Date)
                    .FirstOrDefaultAsync();

                if (lastDate == default)
                {
                    return Ok(new List<ExerciseLog>());
                }

                // Get all logs from that date
                var logs = await _context.ExerciseLogs
                    .Where(l => l.ExerciseId == exerciseId && l.Date == lastDate)
                    .OrderBy(l => l.SetNumber)
                    .ToListAsync();

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching last performance for exercise {ExerciseId}", exerciseId);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    // DTO for reordering exercises
    public class ExerciseOrderUpdate
    {
        public int ExerciseId { get; set; }
        public int NewOrderIndex { get; set; }
    }
}
