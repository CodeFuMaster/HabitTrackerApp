using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HabitTrackerApp.Data;
using HabitTrackerApp.Models;

namespace HabitTrackerApp.Api
{
    /// <summary>
    /// API Controller for category synchronization
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryApiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryApiController> _logger;

        public CategoryApiController(AppDbContext context, ILogger<CategoryApiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {categories.Count} categories for sync");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a single category by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Habits)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return NotFound();
                }

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving category {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] Category category)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created category: {category.Name} (ID: {category.Id})");
                return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            try
            {
                if (id != category.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var existingCategory = await _context.Categories.FindAsync(id);
                if (existingCategory == null)
                {
                    return NotFound();
                }

                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;
                existingCategory.ImageUrl = category.ImageUrl;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated category: {category.Name} (ID: {id})");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating category {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                {
                    return NotFound();
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted category: {category.Name} (ID: {id})");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting category {id}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
