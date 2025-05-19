using CESIZen.Data.Context;
using CESIZen.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CESIZen.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InformationalArticlesController : ControllerBase
{
    private readonly CESIZenDbContext _context;

    public InformationalArticlesController(CESIZenDbContext context)
    {
        _context = context;
    }

    // GET: api/InformationalArticles
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InformationalArticle>>> GetInformationalArticles()
    {
        return await _context.InformationalArticles.ToListAsync();
    }

    // GET: api/InformationalArticles/5
    [HttpGet("{id}")]
    public async Task<ActionResult<InformationalArticle>> GetInformationalArticle(int id)
    {
        var informationalArticle = await _context.InformationalArticles.FindAsync(id);

        if (informationalArticle == null)
        {
            return NotFound();
        }

        return informationalArticle;
    }

    // PUT: api/InformationalArticles/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutInformationalArticle(int id, InformationalArticle informationalArticle)
    {
        if (id != informationalArticle.Id)
        {
            return BadRequest();
        }

        _context.Entry(informationalArticle).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!InformationalArticleExists(id))
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

    // POST: api/InformationalArticles
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<InformationalArticle>> PostInformationalArticle(InformationalArticle informationalArticle)
    {
        _context.InformationalArticles.Add(informationalArticle);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetInformationalArticle", new { id = informationalArticle.Id }, informationalArticle);
    }

    // DELETE: api/InformationalArticles/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteInformationalArticle(int id)
    {
        var informationalArticle = await _context.InformationalArticles.FindAsync(id);
        if (informationalArticle == null)
        {
            return NotFound();
        }

        _context.InformationalArticles.Remove(informationalArticle);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool InformationalArticleExists(int id)
    {
        return _context.InformationalArticles.Any(e => e.Id == id);
    }
}
