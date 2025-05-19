using CESIZen.Data.Context;
using CESIZen.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CESIZen.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RelaxingActivitiesController : ControllerBase
{
    private readonly CESIZenDbContext _context;

    public RelaxingActivitiesController(CESIZenDbContext context)
    {
        _context = context;
    }

    // GET: api/RelaxingActivities
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RelaxingActivity>>> GetRelaxingActivities()
    {
        return await _context.RelaxingActivities.ToListAsync();
    }

    // GET: api/RelaxingActivities/5
    [HttpGet("{id}")]
    public async Task<ActionResult<RelaxingActivity>> GetRelaxingActivity(int id)
    {
        var relaxingActivity = await _context.RelaxingActivities.FindAsync(id);

        if (relaxingActivity == null)
        {
            return NotFound();
        }

        return relaxingActivity;
    }

    // PUT: api/RelaxingActivities/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutRelaxingActivity(int id, RelaxingActivity relaxingActivity)
    {
        if (id != relaxingActivity.Id)
        {
            return BadRequest();
        }

        _context.Entry(relaxingActivity).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RelaxingActivityExists(id))
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

    // POST: api/RelaxingActivities
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<RelaxingActivity>> PostRelaxingActivity(RelaxingActivity relaxingActivity)
    {
        _context.RelaxingActivities.Add(relaxingActivity);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetRelaxingActivity", new { id = relaxingActivity.Id }, relaxingActivity);
    }

    // DELETE: api/RelaxingActivities/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRelaxingActivity(int id)
    {
        var relaxingActivity = await _context.RelaxingActivities.FindAsync(id);
        if (relaxingActivity == null)
        {
            return NotFound();
        }

        _context.RelaxingActivities.Remove(relaxingActivity);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool RelaxingActivityExists(int id)
    {
        return _context.RelaxingActivities.Any(e => e.Id == id);
    }
}
