using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public SuppliersController(InfinityPosDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SupplierDto>>> GetSuppliers()
    {
        var suppliers = await _db.Suppliers
            .AsNoTracking()
            .Where(s => s.IsActive)
            .Select(s => new SupplierDto
            {
                SupplierId = s.SupplierId,
                SupplierCode = s.SupplierCode,
                SupplierName = s.SupplierName,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return Ok(suppliers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SupplierDto>> GetSupplier(int id)
    {
        var supplier = await _db.Suppliers
            .AsNoTracking()
            .Where(s => s.SupplierId == id)
            .Select(s => new SupplierDto
            {
                SupplierId = s.SupplierId,
                SupplierCode = s.SupplierCode,
                SupplierName = s.SupplierName,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (supplier == null)
        {
            return NotFound();
        }

        return Ok(supplier);
    }

    [HttpPost]
    public async Task<ActionResult<SupplierDto>> CreateSupplier(
        CreateSupplierDto dto)
    {
        var codeExists = await _db.Suppliers
            .AnyAsync(s => s.SupplierCode == dto.SupplierCode);

        if (codeExists)
        {
            return BadRequest(
                $"Supplier Code '{dto.SupplierCode}' already exists."
            );
        }

        var supplier = new Supplier
        {
            SupplierCode = dto.SupplierCode,
            SupplierName = dto.SupplierName,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            IsActive = true
        };

        _db.Suppliers.Add(supplier);

        await _db.SaveChangesAsync();

        var result = new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            SupplierCode = supplier.SupplierCode,
            SupplierName = supplier.SupplierName,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetSupplier),
            new { id = supplier.SupplierId },
            result
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<SupplierDto>> UpdateSupplier(
        int id,
        UpdateSupplierDto dto)
    {
        var supplier = await _db.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierId == id);

        if (supplier == null)
        {
            return NotFound();
        }

        var codeExists = await _db.Suppliers
            .AnyAsync(s =>
                s.SupplierCode == dto.SupplierCode &&
                s.SupplierId != id);

        if (codeExists)
        {
            return BadRequest(
                $"Supplier Code '{dto.SupplierCode}' already exists."
            );
        }

        supplier.SupplierCode = dto.SupplierCode;
        supplier.SupplierName = dto.SupplierName;
        supplier.Phone = dto.Phone;
        supplier.Email = dto.Email;
        supplier.Address = dto.Address;
        supplier.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        var result = new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            SupplierCode = supplier.SupplierCode,
            SupplierName = supplier.SupplierName,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt
        };

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var supplier = await _db.Suppliers
            .FirstOrDefaultAsync(s => s.SupplierId == id);

        if (supplier == null)
        {
            return NotFound();
        }

        supplier.IsActive = false;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}