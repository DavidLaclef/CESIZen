﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CESIZen.Data.Context;
using CESIZen.Data.Entities;

namespace CESIZen.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly CESIZenDbContext _context;

    public StatisticsController(CESIZenDbContext context)
    {
        _context = context;
    }

    // GET: api/Statistics
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Statistic>>> GetStatistics()
    {
        return await _context.Statistics.ToListAsync();
    }

    // GET: api/Statistics/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Statistic>> GetStatistic(int id)
    {
        var statistic = await _context.Statistics.FindAsync(id);

        if (statistic == null)
        {
            return NotFound();
        }

        return statistic;
    }

    // PUT: api/Statistics/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> PutStatistic(int id, Statistic statistic)
    {
        if (id != statistic.Id)
        {
            return BadRequest();
        }

        _context.Entry(statistic).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!StatisticExists(id))
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

    // POST: api/Statistics
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Statistic>> PostStatistic(Statistic statistic)
    {
        _context.Statistics.Add(statistic);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetStatistic", new { id = statistic.Id }, statistic);
    }

    // DELETE: api/Statistics/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStatistic(int id)
    {
        var statistic = await _context.Statistics.FindAsync(id);
        if (statistic == null)
        {
            return NotFound();
        }

        _context.Statistics.Remove(statistic);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool StatisticExists(int id)
    {
        return _context.Statistics.Any(e => e.Id == id);
    }
}
