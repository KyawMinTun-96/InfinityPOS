using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public WarehousesController(InfinityPosDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetWarehouses()
    {
        var warehouses = await _db.Warehouses
            .AsNoTracking()
            .Where(w => w.IsActive)
            .Select(w => new WarehouseDto
            {
                WarehouseId = w.WarehouseId,
                WarehouseCode = w.WarehouseCode,
                WarehouseName = w.WarehouseName,
                Address = w.Address,
                IsActive = w.IsActive,
                CreatedAt = w.CreatedAt
            })
            .ToListAsync();

        return Ok(warehouses);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WarehouseDto>> GetWarehouse(int id)
    {
        var warehouse = await _db.Warehouses
            .AsNoTracking()
            .Where(w => w.WarehouseId == id)
            .Select(w => new WarehouseDto
            {
                WarehouseId = w.WarehouseId,
                WarehouseCode = w.WarehouseCode,
                WarehouseName = w.WarehouseName,
                Address = w.Address,
                IsActive = w.IsActive,
                CreatedAt = w.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (warehouse == null)
        {
            return NotFound();
        }

        return Ok(warehouse);
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> CreateWarehouse(
        CreateWarehouseDto dto)
    {
        var codeExists = await _db.Warehouses
            .AnyAsync(w => w.WarehouseCode == dto.WarehouseCode);

        if (codeExists)
        {
            return BadRequest(
                $"Warehouse Code '{dto.WarehouseCode}' already exists."
            );
        }

        var warehouse = new Warehouse
        {
            WarehouseCode = dto.WarehouseCode,
            WarehouseName = dto.WarehouseName,
            Address = dto.Address,
            IsActive = true
        };

        _db.Warehouses.Add(warehouse);

        await _db.SaveChangesAsync();

        var result = new WarehouseDto
        {
            WarehouseId = warehouse.WarehouseId,
            WarehouseCode = warehouse.WarehouseCode,
            WarehouseName = warehouse.WarehouseName,
            Address = warehouse.Address,
            IsActive = warehouse.IsActive,
            CreatedAt = warehouse.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetWarehouse),
            new { id = warehouse.WarehouseId },
            result
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<WarehouseDto>> UpdateWarehouse(
        int id,
        UpdateWarehouseDto dto)
    {
        var warehouse = await _db.Warehouses
            .FirstOrDefaultAsync(w => w.WarehouseId == id);

        if (warehouse == null)
        {
            return NotFound();
        }

        var codeExists = await _db.Warehouses
            .AnyAsync(w =>
                w.WarehouseCode == dto.WarehouseCode &&
                w.WarehouseId != id);

        if (codeExists)
        {
            return BadRequest(
                $"Warehouse Code '{dto.WarehouseCode}' already exists."
            );
        }

        warehouse.WarehouseCode = dto.WarehouseCode;
        warehouse.WarehouseName = dto.WarehouseName;
        warehouse.Address = dto.Address;
        warehouse.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        var result = new WarehouseDto
        {
            WarehouseId = warehouse.WarehouseId,
            WarehouseCode = warehouse.WarehouseCode,
            WarehouseName = warehouse.WarehouseName,
            Address = warehouse.Address,
            IsActive = warehouse.IsActive,
            CreatedAt = warehouse.CreatedAt
        };

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWarehouse(int id)
    {
        var warehouse = await _db.Warehouses
            .FirstOrDefaultAsync(w => w.WarehouseId == id);

        if (warehouse == null)
        {
            return NotFound();
        }

        warehouse.IsActive = false;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}




