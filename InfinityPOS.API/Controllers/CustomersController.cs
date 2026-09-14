using InfinityPOS.API.Data;
using InfinityPOS.API.Data.Models;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly InfinityPosDbContext _db;

    public CustomersController(InfinityPosDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomers()
    {
        var customers = await _db.Customers
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                CustomerTypeId = c.CustomerTypeId,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                CreditLimit = c.CreditLimit,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetCustomer(int id)
    {
        var customer = await _db.Customers
            .AsNoTracking()
            .Where(c => c.CustomerId == id)
            .Select(c => new CustomerDto
            {
                CustomerId = c.CustomerId,
                CustomerCode = c.CustomerCode,
                CustomerName = c.CustomerName,
                CustomerTypeId = c.CustomerTypeId,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address,
                CreditLimit = c.CreditLimit,
                IsActive = c.IsActive,
                CreatedAt = c.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (customer == null)
        {
            return NotFound();
        }

        return Ok(customer);
    }

    [HttpPost]
    public async Task<ActionResult<CustomerDto>> CreateCustomer(
        CreateCustomerDto dto)
    {
        if (dto.CreditLimit < 0)
        {
            return BadRequest("CreditLimit cannot be negative.");
        }

        var codeExists = await _db.Customers
            .AnyAsync(c => c.CustomerCode == dto.CustomerCode);

        if (codeExists)
        {
            return BadRequest(
                $"Customer Code '{dto.CustomerCode}' already exists."
            );
        }

        if (dto.CustomerTypeId.HasValue)
        {
            var customerTypeExists = await _db.CustomerTypes
                .AnyAsync(c =>
                    c.CustomerTypeId == dto.CustomerTypeId.Value &&
                    c.IsActive);

            if (!customerTypeExists)
            {
                return BadRequest(
                    $"Customer Type ID {dto.CustomerTypeId} not found."
                );
            }
        }

        var customer = new Customer
        {
            CustomerCode = dto.CustomerCode,
            CustomerName = dto.CustomerName,
            CustomerTypeId = dto.CustomerTypeId,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            CreditLimit = dto.CreditLimit,
            IsActive = true
        };

        _db.Customers.Add(customer);

        await _db.SaveChangesAsync();

        var result = new CustomerDto
        {
            CustomerId = customer.CustomerId,
            CustomerCode = customer.CustomerCode,
            CustomerName = customer.CustomerName,
            CustomerTypeId = customer.CustomerTypeId,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            CreditLimit = customer.CreditLimit,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetCustomer),
            new { id = customer.CustomerId },
            result
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<CustomerDto>> UpdateCustomer(
        int id,
        UpdateCustomerDto dto)
    {
        if (dto.CreditLimit < 0)
        {
            return BadRequest("CreditLimit cannot be negative.");
        }

        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == id);

        if (customer == null)
        {
            return NotFound();
        }

        var codeExists = await _db.Customers
            .AnyAsync(c =>
                c.CustomerCode == dto.CustomerCode &&
                c.CustomerId != id);

        if (codeExists)
        {
            return BadRequest(
                $"Customer Code '{dto.CustomerCode}' already exists."
            );
        }

        if (dto.CustomerTypeId.HasValue)
        {
            var customerTypeExists = await _db.CustomerTypes
                .AnyAsync(c =>
                    c.CustomerTypeId == dto.CustomerTypeId.Value &&
                    c.IsActive);

            if (!customerTypeExists)
            {
                return BadRequest(
                    $"Customer Type ID {dto.CustomerTypeId} not found."
                );
            }
        }

        customer.CustomerCode = dto.CustomerCode;
        customer.CustomerName = dto.CustomerName;
        customer.CustomerTypeId = dto.CustomerTypeId;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;
        customer.Address = dto.Address;
        customer.CreditLimit = dto.CreditLimit;
        customer.IsActive = dto.IsActive;

        await _db.SaveChangesAsync();

        var result = new CustomerDto
        {
            CustomerId = customer.CustomerId,
            CustomerCode = customer.CustomerCode,
            CustomerName = customer.CustomerName,
            CustomerTypeId = customer.CustomerTypeId,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            CreditLimit = customer.CreditLimit,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };

        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.CustomerId == id);

        if (customer == null)
        {
            return NotFound();
        }

        customer.IsActive = false;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}