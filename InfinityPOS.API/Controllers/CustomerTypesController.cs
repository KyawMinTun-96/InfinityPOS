using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerTypesController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public CustomerTypesController(InfinityPosDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerTypeDto>>> GetCustomerTypes()
    {
        var customerTypes = await _db.CustomerTypes
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new CustomerTypeDto
            {
                CustomerTypeId = c.CustomerTypeId,
                TypeCode = c.TypeCode,
                TypeName = c.TypeName,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(customerTypes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerTypeDto>> GetCustomerType(int id)
    {
        var customerType = await _db.CustomerTypes
            .AsNoTracking()
            .Where(c => c.CustomerTypeId == id)
            .Select(c => new CustomerTypeDto
            {
                CustomerTypeId = c.CustomerTypeId,
                TypeCode = c.TypeCode,
                TypeName = c.TypeName,
                IsActive = c.IsActive
            })
            .FirstOrDefaultAsync();

        if (customerType == null)
        {
            return NotFound();
        }

        return Ok(customerType);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerTypeDto>> CreateCustomerType(
        CreateCustomerTypeDto dto)
    {
        var codeExists = await _db.CustomerTypes
            .AnyAsync(c => c.TypeCode == dto.TypeCode);

        if (codeExists)
        {
            return BadRequest($"Customer Type Code '{dto.TypeCode}' already exists.");
        }

        var customerType = new CustomerType
        {
            TypeCode = dto.TypeCode,
            TypeName = dto.TypeName,
            IsActive = true
        };

        _db.CustomerTypes.Add(customerType);

        await _db.SaveChangesAsync();

        var result = new CustomerTypeDto
        {
            CustomerTypeId = customerType.CustomerTypeId,
            TypeCode = customerType.TypeCode,
            TypeName = customerType.TypeName,
            IsActive = customerType.IsActive
        };

        return CreatedAtAction(
            nameof(GetCustomerType),
            new { id = customerType.CustomerTypeId },
            result
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerTypeDto>> UpdateCustomerType(
        int id,
        UpdateCustomerTypeDto dto)
    {
        var customerType = await _db.CustomerTypes
            .FirstOrDefaultAsync(c => c.CustomerTypeId == id);

        if (customerType == null)
        {
            return NotFound();
        }

        var codeExists = await _db.CustomerTypes
            .AnyAsync(c =>
                c.TypeCode == dto.TypeCode &&
                c.CustomerTypeId != id);

        if (codeExists)
        {
            return BadRequest($"Customer Type Code '{dto.TypeCode}' already exists.");
        }

        customerType.TypeCode = dto.TypeCode;
        customerType.TypeName = dto.TypeName;
        customerType.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        var result = new CustomerTypeDto
        {
            CustomerTypeId = customerType.CustomerTypeId,
            TypeCode = customerType.TypeCode,
            TypeName = customerType.TypeName,
            IsActive = customerType.IsActive
        };

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomerType(int id)
    {
        var customerType = await _db.CustomerTypes
            .FirstOrDefaultAsync(c => c.CustomerTypeId == id);

        if (customerType == null)
        {
            return NotFound();
        }

        customerType.IsActive = false;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}