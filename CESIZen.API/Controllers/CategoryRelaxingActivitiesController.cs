using CESIZen.Data.Context;
using CESIZen.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CESIZen.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryRelaxingActivitiesController : ControllerBase
{
    private readonly CESIZenDbContext _context;

    public CategoryRelaxingActivitiesController(CESIZenDbContext context)
    {
        _context = context;
    }

    // GET: api/CategoryRelaxingActivities
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryRelaxingActivity>>> GetCategoriesRelaxingActivity()
    {
        return await _context.CategoriesRelaxingActivity.ToListAsync();
    }

    // GET: api/CategoryRelaxingActivities/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryRelaxingActivity>> GetCategoryRelaxingActivity(int id)
    {
        var categoryRelaxingActivity = await _context.CategoriesRelaxingActivity.FindAsync(id);

        if (categoryRelaxingActivity == null)
        {
            return NotFound();
        }

        return categoryRelaxingActivity;
    }

    // PUT: api/CategoryRelaxingActivities/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCategoryRelaxingActivity(int id, CategoryRelaxingActivity categoryRelaxingActivity)
    {
        if (id != categoryRelaxingActivity.Id)
        {
            return BadRequest();
        }

        _context.Entry(categoryRelaxingActivity).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CategoryRelaxingActivityExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/CategoryRelaxingActivities
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<CategoryRelaxingActivity>> PostCategoryRelaxingActivity(CategoryRelaxingActivity categoryRelaxingActivity)
    {
        _context.CategoriesRelaxingActivity.Add(categoryRelaxingActivity);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCategoryRelaxingActivity", new { id = categoryRelaxingActivity.Id }, categoryRelaxingActivity);
    }

    // DELETE: api/CategoryRelaxingActivities/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategoryRelaxingActivity(int id)
    {
        var categoryRelaxingActivity = await _context.CategoriesRelaxingActivity.FindAsync(id);
        if (categoryRelaxingActivity == null)
        {
            return NotFound();
        }

        _context.CategoriesRelaxingActivity.Remove(categoryRelaxingActivity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CategoryRelaxingActivityExists(int id)
    {
        return _context.CategoriesRelaxingActivity.Any(e => e.Id == id);
    }
}
