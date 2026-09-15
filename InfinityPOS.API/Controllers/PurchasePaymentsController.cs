using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchasePaymentsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public PurchasePaymentsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/purchasepayments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchasePaymentDto>>> GetPurchasePayments()
    {
        var payments = await _context.PurchasePayments
            .OrderByDescending(x => x.PurchasePaymentId)
            .Select(x => new PurchasePaymentDto
            {
                PurchasePaymentId = x.PurchasePaymentId,
                PurchaseInvoiceId = x.PurchaseInvoiceId,
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

    // GET: api/purchasepayments/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<PurchasePaymentDto>> GetPurchasePayment(long id)
    {
        var payment = await _context.PurchasePayments
            .Where(x => x.PurchasePaymentId == id)
            .Select(x => new PurchasePaymentDto
            {
                PurchasePaymentId = x.PurchasePaymentId,
                PurchaseInvoiceId = x.PurchaseInvoiceId,
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
                message = "Purchase payment not found."
            });
        }

        return Ok(payment);
    }

    // GET: api/purchasepayments/invoice/1
    [HttpGet("invoice/{purchaseInvoiceId:long}")]
    public async Task<ActionResult<IEnumerable<PurchasePaymentDto>>> GetPaymentsByInvoice(
        long purchaseInvoiceId)
    {
        var invoice = await _context.PurchaseInvoices
            .FirstOrDefaultAsync(x => x.PurchaseInvoiceId == purchaseInvoiceId);

        if (invoice == null)
        {
            return NotFound(new
            {
                message = "Purchase invoice not found."
            });
        }

        var payments = await _context.PurchasePayments
            .Where(x => x.PurchaseInvoiceId == purchaseInvoiceId)
            .OrderBy(x => x.PurchasePaymentId)
            .Select(x => new PurchasePaymentDto
            {
                PurchasePaymentId = x.PurchasePaymentId,
                PurchaseInvoiceId = x.PurchaseInvoiceId,
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

    // POST: api/purchasepayments
    [HttpPost]
    public async Task<ActionResult<PurchasePaymentDto>> CreatePurchasePayment(
        CreatePurchasePaymentDto dto)
    {
        if (dto.PurchaseInvoiceId <= 0)
        {
            return BadRequest(new
            {
                message = "PurchaseInvoiceId is required."
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
                message = "Amount must be greater than 0."
            });
        }

        var invoice = await _context.PurchaseInvoices
            .FirstOrDefaultAsync(x =>
                x.PurchaseInvoiceId == dto.PurchaseInvoiceId);

        if (invoice == null)
        {
            return BadRequest(new
            {
                message = "Purchase invoice not found."
            });
        }

        // Payment Method validation
        var paymentMethodExists = await _context.PaymentMethods
            .AnyAsync(x =>
                x.PaymentMethodId == dto.PaymentMethodId &&
                x.IsActive);

        if (!paymentMethodExists)
        {
            return BadRequest(new
            {
                message = "Payment method not found or inactive."
            });
        }

        // Existing payments
        var paidAmount = await _context.PurchasePayments
            .Where(x => x.PurchaseInvoiceId == dto.PurchaseInvoiceId)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        var remainingAmount = invoice.TotalAmount - paidAmount;

        if (dto.Amount > remainingAmount)
        {
            return BadRequest(new
            {
                message = "Payment amount exceeds remaining invoice amount.",
                invoiceTotal = invoice.TotalAmount,
                alreadyPaid = paidAmount,
                remaining = remainingAmount
            });
        }

        var payment = new Data.Models.PurchasePayment
        {
            PurchaseInvoiceId = dto.PurchaseInvoiceId,
            PaymentMethodId = dto.PaymentMethodId,
            Amount = dto.Amount,
            PaymentDate = dto.PaymentDate ?? DateTime.Now,
            ReferenceNumber = dto.ReferenceNumber?.Trim(),
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.Now
        };

        _context.PurchasePayments.Add(payment);

        await _context.SaveChangesAsync();

        var result = new PurchasePaymentDto
        {
            PurchasePaymentId = payment.PurchasePaymentId,
            PurchaseInvoiceId = payment.PurchaseInvoiceId,
            PaymentMethodId = payment.PaymentMethodId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetPurchasePayment),
            new { id = payment.PurchasePaymentId },
            result
        );
    }

    // PUT: api/purchasepayments/1
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdatePurchasePayment(
        long id,
        UpdatePurchasePaymentDto dto)
    {
        if (dto.PurchaseInvoiceId <= 0)
        {
            return BadRequest(new
            {
                message = "PurchaseInvoiceId is required."
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
                message = "Amount must be greater than 0."
            });
        }

        var payment = await _context.PurchasePayments
            .FirstOrDefaultAsync(x => x.PurchasePaymentId == id);

        if (payment == null)
        {
            return NotFound(new
            {
                message = "Purchase payment not found."
            });
        }

        var invoice = await _context.PurchaseInvoices
            .FirstOrDefaultAsync(x =>
                x.PurchaseInvoiceId == dto.PurchaseInvoiceId);

        if (invoice == null)
        {
            return BadRequest(new
            {
                message = "Purchase invoice not found."
            });
        }

        var paymentMethodExists = await _context.PaymentMethods
            .AnyAsync(x =>
                x.PaymentMethodId == dto.PaymentMethodId &&
                x.IsActive);

        if (!paymentMethodExists)
        {
            return BadRequest(new
            {
                message = "Payment method not found or inactive."
            });
        }

        // Exclude current payment when calculating remaining amount
        var paidAmount = await _context.PurchasePayments
            .Where(x =>
                x.PurchaseInvoiceId == dto.PurchaseInvoiceId &&
                x.PurchasePaymentId != id)
            .SumAsync(x => (decimal?)x.Amount) ?? 0;

        var remainingAmount = invoice.TotalAmount - paidAmount;

        if (dto.Amount > remainingAmount)
        {
            return BadRequest(new
            {
                message = "Payment amount exceeds remaining invoice amount.",
                invoiceTotal = invoice.TotalAmount,
                alreadyPaid = paidAmount,
                remaining = remainingAmount
            });
        }

        payment.PurchaseInvoiceId = dto.PurchaseInvoiceId;
        payment.PaymentMethodId = dto.PaymentMethodId;
        payment.Amount = dto.Amount;
        payment.PaymentDate = dto.PaymentDate;
        payment.ReferenceNumber = dto.ReferenceNumber?.Trim();
        payment.Notes = dto.Notes?.Trim();

        await _context.SaveChangesAsync();

        return Ok(new PurchasePaymentDto
        {
            PurchasePaymentId = payment.PurchasePaymentId,
            PurchaseInvoiceId = payment.PurchaseInvoiceId,
            PaymentMethodId = payment.PaymentMethodId,
            Amount = payment.Amount,
            PaymentDate = payment.PaymentDate,
            ReferenceNumber = payment.ReferenceNumber,
            Notes = payment.Notes,
            CreatedAt = payment.CreatedAt
        });
    }

    // DELETE: api/purchasepayments/1
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeletePurchasePayment(long id)
    {
        var payment = await _context.PurchasePayments
            .FirstOrDefaultAsync(x => x.PurchasePaymentId == id);

        if (payment == null)
        {
            return NotFound(new
            {
                message = "Purchase payment not found."
            });
        }

        _context.PurchasePayments.Remove(payment);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Purchase payment deleted successfully."
        });
    }
}