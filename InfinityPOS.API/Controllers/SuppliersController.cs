using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SuppliersController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public SuppliersController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // ============================================================
    // GET: api/suppliers
    // GET: api/suppliers?isActive=true
    // ============================================================
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Supplier>>> GetSuppliers(
        [FromQuery] bool? isActive = null)
    {
        IQueryable<Supplier> query = _context.Suppliers
            .AsNoTracking();

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var suppliers = await query
            .OrderBy(x => x.SupplierName)
            .ToListAsync();

        return Ok(suppliers);
    }

    // ============================================================
    // GET: api/suppliers/5
    // ============================================================
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Supplier>> GetSupplier(int id)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SupplierId == id);

        if (supplier == null)
        {
            return NotFound(new
            {
                message = "Supplier not found."
            });
        }

        return Ok(supplier);
    }

    // ============================================================
    // POST: api/suppliers
    // ============================================================
    [HttpPost]
    public async Task<ActionResult<Supplier>> CreateSupplier(
        [FromBody] Supplier supplier)
    {
        if (supplier == null)
        {
            return BadRequest(new
            {
                message = "Supplier data is required."
            });
        }

        if (string.IsNullOrWhiteSpace(supplier.SupplierCode))
        {
            return BadRequest(new
            {
                message = "Supplier code is required."
            });
        }

        if (string.IsNullOrWhiteSpace(supplier.SupplierName))
        {
            return BadRequest(new
            {
                message = "Supplier name is required."
            });
        }

        var codeExists = await _context.Suppliers
            .AnyAsync(x => x.SupplierCode == supplier.SupplierCode);

        if (codeExists)
        {
            return Conflict(new
            {
                message = "Supplier code already exists."
            });
        }

        supplier.SupplierId = 0;
        supplier.CreatedAt = DateTime.Now;

        _context.Suppliers.Add(supplier);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetSupplier),
            new { id = supplier.SupplierId },
            supplier);
    }

    // ============================================================
    // PUT: api/suppliers/5
    // ============================================================
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateSupplier(
        int id,
        [FromBody] Supplier supplier)
    {
        if (supplier == null)
        {
            return BadRequest(new
            {
                message = "Supplier data is required."
            });
        }

        var existingSupplier = await _context.Suppliers
            .FirstOrDefaultAsync(x => x.SupplierId == id);

        if (existingSupplier == null)
        {
            return NotFound(new
            {
                message = "Supplier not found."
            });
        }

        if (string.IsNullOrWhiteSpace(supplier.SupplierCode))
        {
            return BadRequest(new
            {
                message = "Supplier code is required."
            });
        }

        if (string.IsNullOrWhiteSpace(supplier.SupplierName))
        {
            return BadRequest(new
            {
                message = "Supplier name is required."
            });
        }

        var codeExists = await _context.Suppliers
            .AnyAsync(x =>
                x.SupplierId != id &&
                x.SupplierCode == supplier.SupplierCode);

        if (codeExists)
        {
            return Conflict(new
            {
                message = "Supplier code already exists."
            });
        }

        existingSupplier.SupplierCode = supplier.SupplierCode;
        existingSupplier.SupplierName = supplier.SupplierName;
        existingSupplier.Phone = supplier.Phone;
        existingSupplier.Email = supplier.Email;
        existingSupplier.Address = supplier.Address;
        existingSupplier.IsActive = supplier.IsActive;

        await _context.SaveChangesAsync();

        return Ok(existingSupplier);
    }

    // ============================================================
    // DELETE: api/suppliers/5
    // Soft Delete
    // ============================================================
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteSupplier(int id)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(x => x.SupplierId == id);

        if (supplier == null)
        {
            return NotFound(new
            {
                message = "Supplier not found."
            });
        }

        supplier.IsActive = false;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}