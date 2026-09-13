using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UnitsController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public UnitsController(InfinityPosDbContext db)
    {
        _db = db;
    }

    // GET: api/units
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UnitDto>>> GetUnits()
    {
        var units = await _db.Units
            .AsNoTracking()
            .Where(u => u.IsActive)
            .Select(u => new UnitDto
            {
                UnitId = u.UnitId,
                UnitCode = u.UnitCode,
                UnitName = u.UnitName,
                IsActive = u.IsActive
            })
            .ToListAsync();

        return Ok(units);
    }

    // GET: api/units/1
    [HttpGet("{id}")]
    public async Task<ActionResult<UnitDto>> GetUnit(int id)
    {
        var unit = await _db.Units
            .AsNoTracking()
            .Where(u => u.UnitId == id)
            .Select(u => new UnitDto
            {
                UnitId = u.UnitId,
                UnitCode = u.UnitCode,
                UnitName = u.UnitName,
                IsActive = u.IsActive
            })
            .FirstOrDefaultAsync();

        if (unit == null)
        {
            return NotFound();
        }

        return Ok(unit);
    }

    // POST: api/units
    [HttpPost]
    public async Task<ActionResult<UnitDto>> CreateUnit(
        CreateUnitDto dto)
    {
        var unit = new Unit
        {
            UnitCode = dto.UnitCode,
            UnitName = dto.UnitName,
            IsActive = true
        };

        _db.Units.Add(unit);

        await _db.SaveChangesAsync();

        var result = new UnitDto
        {
            UnitId = unit.UnitId,
            UnitCode = unit.UnitCode,
            UnitName = unit.UnitName,
            IsActive = unit.IsActive
        };

        return CreatedAtAction(
            nameof(GetUnit),
            new { id = unit.UnitId },
            result
        );
    }

    // PUT: api/units/1
    [HttpPut("{id}")]
    public async Task<ActionResult<UnitDto>> UpdateUnit(
        int id,
        UpdateUnitDto dto)
    {
        var unit = await _db.Units
            .FirstOrDefaultAsync(u => u.UnitId == id);

        if (unit == null)
        {
            return NotFound();
        }

        unit.UnitCode = dto.UnitCode;
        unit.UnitName = dto.UnitName;
        unit.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        var result = new UnitDto
        {
            UnitId = unit.UnitId,
            UnitCode = unit.UnitCode,
            UnitName = unit.UnitName,
            IsActive = unit.IsActive
        };

        return Ok(result);
    }

    // DELETE: api/units/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUnit(int id)
    {
        var unit = await _db.Units
            .FirstOrDefaultAsync(u => u.UnitId == id);

        if (unit == null)
        {
            return NotFound();
        }

        // Soft Delete
        unit.IsActive = false;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}