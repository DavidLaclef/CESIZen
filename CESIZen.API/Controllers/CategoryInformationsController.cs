using CESIZen.Data.Context;
using CESIZen.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CESIZen.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryInformationsController : ControllerBase
{
    private readonly CESIZenDbContext _context;

    public CategoryInformationsController(CESIZenDbContext context)
    {
        _context = context;
    }

    // GET: api/CategoryInformations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryInformation>>> GetCategoriesInformation()
    {
        return await _context.CategoriesInformation.ToListAsync();
    }

    // GET: api/CategoryInformations/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryInformation>> GetCategoryInformation(int id)
    {
        var categoryInformation = await _context.CategoriesInformation.FindAsync(id);

        if (categoryInformation == null)
        {
            return NotFound();
        }

        return categoryInformation;
    }

    // PUT: api/CategoryInformations/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutCategoryInformation(int id, CategoryInformation categoryInformation)
    {
        if (id != categoryInformation.Id)
        {
            return BadRequest();
        }

        _context.Entry(categoryInformation).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CategoryInformationExists(id))
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

    // POST: api/CategoryInformations
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<CategoryInformation>> PostCategoryInformation(CategoryInformation categoryInformation)
    {
        _context.CategoriesInformation.Add(categoryInformation);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetCategoryInformation", new { id = categoryInformation.Id }, categoryInformation);
    }

    // DELETE: api/CategoryInformations/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategoryInformation(int id)
    {
        var categoryInformation = await _context.CategoriesInformation.FindAsync(id);
        if (categoryInformation == null)
        {
            return NotFound();
        }

        _context.CategoriesInformation.Remove(categoryInformation);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool CategoryInformationExists(int id)
    {
        return _context.CategoriesInformation.Any(e => e.Id == id);
    }
}
