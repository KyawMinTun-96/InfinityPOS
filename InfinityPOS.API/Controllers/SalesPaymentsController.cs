using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesPaymentsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public SalesPaymentsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/salespayments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesPaymentDto>>> GetAll()
    {
        var payments = await _context.SalesPayments
            .AsNoTracking()
            .OrderByDescending(x => x.SalesPaymentId)
            .Select(x => new SalesPaymentDto
            {
                SalesPaymentId = x.SalesPaymentId,
                SalesInvoiceId = x.SalesInvoiceId,
                PaymentMethodId = x.PaymentMethodId,
                Amount = x.Amount,
                PaymentDate = x.PaymentDate,
                ReferenceNumber = x.ReferenceNumber,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(payments);
    }

    // GET: api/salespayments/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<SalesPaymentDto>> GetById(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid SalesPaymentId."
            });
        }

        var payment = await _context.SalesPayments
            .AsNoTracking()
            .Where(x => x.SalesPaymentId == id)
            .Select(x => new SalesPaymentDto
            {
                SalesPaymentId = x.SalesPaymentId,
                SalesInvoiceId = x.SalesInvoiceId,
                PaymentMethodId = x.PaymentMethodId,
                Amount = x.Amount,
                PaymentDate = x.PaymentDate,
                ReferenceNumber = x.ReferenceNumber,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (payment == null)
        {
            return NotFound(new
            {
                message = "Sales payment not found."
            });
        }

        return Ok(payment);
    }

    // GET: api/salespayments/invoice/1
    [HttpGet("invoice/{salesInvoiceId:long}")]
    public async Task<ActionResult<IEnumerable<SalesPaymentDto>>> GetByInvoice(
        long salesInvoiceId)
    {
        if (salesInvoiceId <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid SalesInvoiceId."
            });
        }

        var invoiceExists = await _context.SalesInvoices
            .AnyAsync(x => x.SalesInvoiceId == salesInvoiceId);

        if (!invoiceExists)
        {
            return NotFound(new
            {
                message = "Sales invoice not found."
            });
        }

        var payments = await _context.SalesPayments
            .AsNoTracking()
            .Where(x => x.SalesInvoiceId == salesInvoiceId)
            .OrderByDescending(x => x.SalesPaymentId)
            .Select(x => new SalesPaymentDto
            {
                SalesPaymentId = x.SalesPaymentId,
                SalesInvoiceId = x.SalesInvoiceId,
                PaymentMethodId = x.PaymentMethodId,
                Amount = x.Amount,
                PaymentDate = x.PaymentDate,
                ReferenceNumber = x.ReferenceNumber,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(payments);
    }

    // POST: api/salespayments
    [HttpPost]
    public async Task<ActionResult<SalesPaymentDto>> Create(
        CreateSalesPaymentDto dto)
    {
        if (dto.SalesInvoiceId <= 0)
        {
            return BadRequest(new
            {
                message = "SalesInvoiceId is required."
            });
        }

        if (dto.PaymentMethodId <= 0)
        {
            return BadRequest(new
            {
                message = "PaymentMethodId is required."
            });
        }

        if (dto.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "Payment Amount must be greater than 0."
            });
        }

        var invoice = await _context.SalesInvoices
            .FirstOrDefaultAsync(x =>
                x.SalesInvoiceId == dto.SalesInvoiceId);

        if (invoice == null)
        {
            return NotFound(new
            {
                message = "Sales invoice not found."
            });
        }

        // Only POSTED invoice can receive payment
        const int postedStatusId = 2;

        if (invoice.DocumentStatusId != postedStatusId)
        {
            return BadRequest(new
            {
                message = "Only posted sales invoices can receive payment."
            });
        }

        var paymentMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(x =>
                x.PaymentMethodId == dto.PaymentMethodId &&
                x.IsActive);

        if (paymentMethod == null)
        {
            return BadRequest(new
            {
                message = "Payment method not found or inactive."
            });
        }

        // Existing payments
        var existingPayments = await _context.SalesPayments
            .Where(x =>
                x.SalesInvoiceId == dto.SalesInvoiceId)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        var remainingAmount =
            invoice.TotalAmount - existingPayments;

        if (dto.Amount > remainingAmount)
        {
            return BadRequest(new
            {
                message = "Payment amount exceeds remaining invoice balance.",
                invoiceTotal = invoice.TotalAmount,
                paidAmount = existingPayments,
                remainingAmount = remainingAmount,
                requestedAmount = dto.Amount
            });
        }

        var payment = new Data.Models.SalesPayment
        {
            SalesInvoiceId = dto.SalesInvoiceId,
            PaymentMethodId = dto.PaymentMethodId,
            Amount = dto.Amount,
            PaymentDate = dto.PaymentDate ?? DateTime.Now,
            ReferenceNumber = dto.ReferenceNumber,
            Notes = dto.Notes,
            CreatedAt = DateTime.Now
        };

        _context.SalesPayments.Add(payment);

        await _context.SaveChangesAsync();

        var result = new SalesPaymentDto
        {
            SalesPaymentId = payment.SalesPaymentId,
            SalesInvoiceId = payment.SalesInvoiceId,
            PaymentMethodId = payment.PaymentMethodId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = payment.SalesPaymentId },
            result);
    }

    // PUT: api/salespayments/1
    [HttpPut("{id:long}")]
    public async Task<ActionResult<SalesPaymentDto>> Update(
        long id,
        UpdateSalesPaymentDto dto)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid SalesPaymentId."
            });
        }

        if (dto.SalesInvoiceId <= 0)
        {
            return BadRequest(new
            {
                message = "SalesInvoiceId is required."
            });
        }

        if (dto.PaymentMethodId <= 0)
        {
            return BadRequest(new
            {
                message = "PaymentMethodId is required."
            });
        }

        if (dto.Amount <= 0)
        {
            return BadRequest(new
            {
                message = "Payment Amount must be greater than 0."
            });
        }

        var payment = await _context.SalesPayments
            .FirstOrDefaultAsync(x =>
                x.SalesPaymentId == id);

        if (payment == null)
        {
            return NotFound(new
            {
                message = "Sales payment not found."
            });
        }

        var invoice = await _context.SalesInvoices
            .FirstOrDefaultAsync(x =>
                x.SalesInvoiceId == dto.SalesInvoiceId);

        if (invoice == null)
        {
            return NotFound(new
            {
                message = "Sales invoice not found."
            });
        }

        const int postedStatusId = 2;

        if (invoice.DocumentStatusId != postedStatusId)
        {
            return BadRequest(new
            {
                message = "Only posted sales invoices can receive payment."
            });
        }

        var paymentMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(x =>
                x.PaymentMethodId == dto.PaymentMethodId &&
                x.IsActive);

        if (paymentMethod == null)
        {
            return BadRequest(new
            {
                message = "Payment method not found or inactive."
            });
        }

        // Exclude current payment from total
        var existingPayments = await _context.SalesPayments
            .Where(x =>
                x.SalesInvoiceId == dto.SalesInvoiceId &&
                x.SalesPaymentId != id)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        var remainingAmount =
            invoice.TotalAmount - existingPayments;

        if (dto.Amount > remainingAmount)
        {
            return BadRequest(new
            {
                message = "Payment amount exceeds remaining invoice balance.",
                invoiceTotal = invoice.TotalAmount,
                paidAmount = existingPayments,
                remainingAmount = remainingAmount,
                requestedAmount = dto.Amount
            });
        }

        payment.SalesInvoiceId = dto.SalesInvoiceId;
        payment.PaymentMethodId = dto.PaymentMethodId;
        payment.Amount = dto.Amount;
        payment.PaymentDate = dto.PaymentDate;
        payment.ReferenceNumber = dto.ReferenceNumber;
        payment.Notes = dto.Notes;

        await _context.SaveChangesAsync();

        var result = new SalesPaymentDto
        {
            SalesPaymentId = payment.SalesPaymentId,
            SalesInvoiceId = payment.SalesInvoiceId,
            PaymentMethodId = payment.PaymentMethodId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt
        };

        return Ok(result);
    }

    // DELETE: api/salespayments/1
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid SalesPaymentId."
            });
        }

        var payment = await _context.SalesPayments
            .FirstOrDefaultAsync(x =>
                x.SalesPaymentId == id);

        if (payment == null)
        {
            return NotFound(new
            {
                message = "Sales payment not found."
            });
        }

        _context.SalesPayments.Remove(payment);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Sales payment deleted successfully."
        });
    }
}