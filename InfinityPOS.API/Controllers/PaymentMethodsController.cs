using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentMethodsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public PaymentMethodsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/paymentmethods
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentMethodDto>>> GetPaymentMethods()
    {
        var paymentMethods = await _context.PaymentMethods
            .Where(x => x.IsActive)
            .OrderBy(x => x.PaymentMethodId)
            .Select(x => new PaymentMethodDto
            {
                PaymentMethodId = x.PaymentMethodId,
                MethodCode = x.MethodCode,
                MethodName = x.MethodName,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return Ok(paymentMethods);
    }

    // GET: api/paymentmethods/1
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentMethodDto>> GetPaymentMethod(int id)
    {
        var paymentMethod = await _context.PaymentMethods
            .Where(x => x.PaymentMethodId == id)
            .Select(x => new PaymentMethodDto
            {
                PaymentMethodId = x.PaymentMethodId,
                MethodCode = x.MethodCode,
                MethodName = x.MethodName,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();

        if (paymentMethod == null)
        {
            return NotFound(new
            {
                message = "Payment method not found."
            });
        }

        return Ok(paymentMethod);
    }

    // POST: api/paymentmethods
    [HttpPost]
    public async Task<ActionResult<PaymentMethodDto>> CreatePaymentMethod(
        CreatePaymentMethodDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MethodCode))
        {
            return BadRequest(new
            {
                message = "MethodCode is required."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.MethodName))
        {
            return BadRequest(new
            {
                message = "MethodName is required."
            });
        }

        var methodCode = dto.MethodCode.Trim();

        var exists = await _context.PaymentMethods
            .AnyAsync(x => x.MethodCode == methodCode);

        if (exists)
        {
            return Conflict(new
            {
                message = "MethodCode already exists."
            });
        }

        var paymentMethod = new Data.Models.PaymentMethod
        {
            MethodCode = methodCode,
            MethodName = dto.MethodName.Trim(),
            IsActive = true
        };

        _context.PaymentMethods.Add(paymentMethod);
        await _context.SaveChangesAsync();

        var result = new PaymentMethodDto
        {
            PaymentMethodId = paymentMethod.PaymentMethodId,
            MethodCode = paymentMethod.MethodCode,
            MethodName = paymentMethod.MethodName,
            IsActive = paymentMethod.IsActive
        };

        return CreatedAtAction(
            nameof(GetPaymentMethod),
            new { id = paymentMethod.PaymentMethodId },
            result
        );
    }

    // PUT: api/paymentmethods/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePaymentMethod(
        int id,
        UpdatePaymentMethodDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MethodCode))
        {
            return BadRequest(new
            {
                message = "MethodCode is required."
            });
        }

        if (string.IsNullOrWhiteSpace(dto.MethodName))
        {
            return BadRequest(new
            {
                message = "MethodName is required."
            });
        }

        var paymentMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(x => x.PaymentMethodId == id);

        if (paymentMethod == null)
        {
            return NotFound(new
            {
                message = "Payment method not found."
            });
        }

        var methodCode = dto.MethodCode.Trim();

        var duplicate = await _context.PaymentMethods
            .AnyAsync(x =>
                x.PaymentMethodId != id &&
                x.MethodCode == methodCode);

        if (duplicate)
        {
            return Conflict(new
            {
                message = "MethodCode already exists."
            });
        }

        paymentMethod.MethodCode = methodCode;
        paymentMethod.MethodName = dto.MethodName.Trim();
        paymentMethod.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

        return Ok(new PaymentMethodDto
        {
            PaymentMethodId = paymentMethod.PaymentMethodId,
            MethodCode = paymentMethod.MethodCode,
            MethodName = paymentMethod.MethodName,
            IsActive = paymentMethod.IsActive
        });
    }

    // DELETE: api/paymentmethods/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePaymentMethod(int id)
    {
        var paymentMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(x => x.PaymentMethodId == id);

        if (paymentMethod == null)
        {
            return NotFound(new
            {
                message = "Payment method not found."
            });
        }

        // Soft delete
        paymentMethod.IsActive = false;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Payment method deleted successfully."
        });
    }
}