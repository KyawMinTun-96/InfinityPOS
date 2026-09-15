using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchaseInvoicesController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public PurchaseInvoicesController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/purchaseinvoices
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseInvoiceDto>>> GetPurchaseInvoices()
    {
        var invoices = await _context.PurchaseInvoices
            .OrderByDescending(x => x.PurchaseInvoiceId)
            .Select(x => new PurchaseInvoiceDto
            {
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                InvoiceNumber = x.InvoiceNumber,
                InvoiceDate = x.InvoiceDate,
                SupplierId = x.SupplierId,
                WarehouseId = x.WarehouseId,
                CurrencyId = x.CurrencyId,
                ExchangeRate = x.ExchangeRate,
                SubTotal = x.SubTotal,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount,
                DocumentStatusId = x.DocumentStatusId,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                CreatedByUserId = x.CreatedByUserId
            })
            .ToListAsync();

        return Ok(invoices);
    }

    // GET: api/purchaseinvoices/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<PurchaseInvoiceDto>> GetPurchaseInvoice(long id)
    {
        var invoice = await _context.PurchaseInvoices
            .Where(x => x.PurchaseInvoiceId == id)
            .Select(x => new PurchaseInvoiceDto
            {
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                InvoiceNumber = x.InvoiceNumber,
                InvoiceDate = x.InvoiceDate,
                SupplierId = x.SupplierId,
                WarehouseId = x.WarehouseId,
                CurrencyId = x.CurrencyId,
                ExchangeRate = x.ExchangeRate,
                SubTotal = x.SubTotal,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount,
                DocumentStatusId = x.DocumentStatusId,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                CreatedByUserId = x.CreatedByUserId
            })
            .FirstOrDefaultAsync();

        if (invoice == null)
        {
            return NotFound(new
            {
                message = "Purchase invoice not found."
            });
        }

        return Ok(invoice);
    }

    // POST: api/purchaseinvoices
    [HttpPost]
    public async Task<ActionResult<PurchaseInvoiceDto>> CreatePurchaseInvoice(
        CreatePurchaseInvoiceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.InvoiceNumber))
        {
            return BadRequest(new
            {
                message = "InvoiceNumber is required."
            });
        }

        if (dto.WarehouseId <= 0)
        {
            return BadRequest(new
            {
                message = "WarehouseId is required."
            });
        }

        if (dto.CurrencyId <= 0)
        {
            return BadRequest(new
            {
                message = "CurrencyId is required."
            });
        }

        if (dto.DocumentStatusId <= 0)
        {
            return BadRequest(new
            {
                message = "DocumentStatusId is required."
            });
        }

        if (dto.ExchangeRate <= 0)
        {
            return BadRequest(new
            {
                message = "ExchangeRate must be greater than 0."
            });
        }

        if (dto.SubTotal < 0 ||
            dto.DiscountAmount < 0 ||
            dto.TaxAmount < 0 ||
            dto.TotalAmount < 0)
        {
            return BadRequest(new
            {
                message = "Amounts cannot be negative."
            });
        }

        var invoiceNumber = dto.InvoiceNumber.Trim();

        var duplicate = await _context.PurchaseInvoices
            .AnyAsync(x => x.InvoiceNumber == invoiceNumber);

        if (duplicate)
        {
            return Conflict(new
            {
                message = "InvoiceNumber already exists."
            });
        }

        // Warehouse validation
        var warehouseExists = await _context.Warehouses
            .AnyAsync(x =>
                x.WarehouseId == dto.WarehouseId &&
                x.IsActive);

        if (!warehouseExists)
        {
            return BadRequest(new
            {
                message = "Warehouse not found or inactive."
            });
        }

        // Currency validation
        var currencyExists = await _context.Currencies
            .AnyAsync(x =>
                x.CurrencyId == dto.CurrencyId &&
                x.IsActive);

        if (!currencyExists)
        {
            return BadRequest(new
            {
                message = "Currency not found or inactive."
            });
        }

        // Supplier validation
        if (dto.SupplierId.HasValue)
        {
            var supplierExists = await _context.Suppliers
                .AnyAsync(x =>
                    x.SupplierId == dto.SupplierId.Value &&
                    x.IsActive);

            if (!supplierExists)
            {
                return BadRequest(new
                {
                    message = "Supplier not found or inactive."
                });
            }
        }

        // Document Status validation
        var statusExists = await _context.DocumentStatuses
            .AnyAsync(x =>
                x.DocumentStatusId == dto.DocumentStatusId &&
                x.DocumentType == "PURCHASE");

        if (!statusExists)
        {
            return BadRequest(new
            {
                message = "Invalid Purchase document status."
            });
        }

        // Created By User validation
        if (dto.CreatedByUserId.HasValue)
        {
            var userExists = await _context.Users
                .AnyAsync(x =>
                    x.UserId == dto.CreatedByUserId.Value &&
                    x.IsActive);

            if (!userExists)
            {
                return BadRequest(new
                {
                    message = "CreatedByUserId not found or inactive."
                });
            }
        }

        var invoice = new Data.Models.PurchaseInvoice
        {
            InvoiceNumber = invoiceNumber,
            InvoiceDate = dto.InvoiceDate ?? DateTime.Now,
            SupplierId = dto.SupplierId,
            WarehouseId = dto.WarehouseId,
            CurrencyId = dto.CurrencyId,
            ExchangeRate = dto.ExchangeRate,
            SubTotal = dto.SubTotal,
            DiscountAmount = dto.DiscountAmount,
            TaxAmount = dto.TaxAmount,
            TotalAmount = dto.TotalAmount,
            DocumentStatusId = dto.DocumentStatusId,
            Notes = dto.Notes?.Trim(),
            CreatedAt = DateTime.Now,
            CreatedByUserId = dto.CreatedByUserId
        };

        _context.PurchaseInvoices.Add(invoice);
        await _context.SaveChangesAsync();

        var result = new PurchaseInvoiceDto
        {
            PurchaseInvoiceId = invoice.PurchaseInvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            SupplierId = invoice.SupplierId,
            WarehouseId = invoice.WarehouseId,
            CurrencyId = invoice.CurrencyId,
            ExchangeRate = invoice.ExchangeRate,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            DocumentStatusId = invoice.DocumentStatusId,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            CreatedByUserId = invoice.CreatedByUserId
        };

        return CreatedAtAction(
            nameof(GetPurchaseInvoice),
            new { id = invoice.PurchaseInvoiceId },
            result
        );
    }

    // PUT: api/purchaseinvoices/1
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdatePurchaseInvoice(
        long id,
        UpdatePurchaseInvoiceDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.InvoiceNumber))
        {
            return BadRequest(new
            {
                message = "InvoiceNumber is required."
            });
        }

        if (dto.WarehouseId <= 0 ||
            dto.CurrencyId <= 0 ||
            dto.DocumentStatusId <= 0)
        {
            return BadRequest(new
            {
                message = "WarehouseId, CurrencyId and DocumentStatusId are required."
            });
        }

        if (dto.ExchangeRate <= 0)
        {
            return BadRequest(new
            {
                message = "ExchangeRate must be greater than 0."
            });
        }

        if (dto.SubTotal < 0 ||
            dto.DiscountAmount < 0 ||
            dto.TaxAmount < 0 ||
            dto.TotalAmount < 0)
        {
            return BadRequest(new
            {
                message = "Amounts cannot be negative."
            });
        }

        var invoice = await _context.PurchaseInvoices
            .FirstOrDefaultAsync(x => x.PurchaseInvoiceId == id);

        if (invoice == null)
        {
            return NotFound(new
            {
                message = "Purchase invoice not found."
            });
        }

        var invoiceNumber = dto.InvoiceNumber.Trim();

        var duplicate = await _context.PurchaseInvoices
            .AnyAsync(x =>
                x.PurchaseInvoiceId != id &&
                x.InvoiceNumber == invoiceNumber);

        if (duplicate)
        {
            return Conflict(new
            {
                message = "InvoiceNumber already exists."
            });
        }

        var warehouseExists = await _context.Warehouses
            .AnyAsync(x =>
                x.WarehouseId == dto.WarehouseId &&
                x.IsActive);

        if (!warehouseExists)
        {
            return BadRequest(new
            {
                message = "Warehouse not found or inactive."
            });
        }

        var currencyExists = await _context.Currencies
            .AnyAsync(x =>
                x.CurrencyId == dto.CurrencyId &&
                x.IsActive);

        if (!currencyExists)
        {
            return BadRequest(new
            {
                message = "Currency not found or inactive."
            });
        }

        if (dto.SupplierId.HasValue)
        {
            var supplierExists = await _context.Suppliers
                .AnyAsync(x =>
                    x.SupplierId == dto.SupplierId.Value &&
                    x.IsActive);

            if (!supplierExists)
            {
                return BadRequest(new
                {
                    message = "Supplier not found or inactive."
                });
            }
        }

        var statusExists = await _context.DocumentStatuses
            .AnyAsync(x =>
                x.DocumentStatusId == dto.DocumentStatusId &&
                x.DocumentType == "PURCHASE");

        if (!statusExists)
        {
            return BadRequest(new
            {
                message = "Invalid Purchase document status."
            });
        }

        if (dto.CreatedByUserId.HasValue)
        {
            var userExists = await _context.Users
                .AnyAsync(x =>
                    x.UserId == dto.CreatedByUserId.Value &&
                    x.IsActive);

            if (!userExists)
            {
                return BadRequest(new
                {
                    message = "CreatedByUserId not found or inactive."
                });
            }
        }

        invoice.InvoiceNumber = invoiceNumber;
        invoice.InvoiceDate = dto.InvoiceDate;
        invoice.SupplierId = dto.SupplierId;
        invoice.WarehouseId = dto.WarehouseId;
        invoice.CurrencyId = dto.CurrencyId;
        invoice.ExchangeRate = dto.ExchangeRate;
        invoice.SubTotal = dto.SubTotal;
        invoice.DiscountAmount = dto.DiscountAmount;
        invoice.TaxAmount = dto.TaxAmount;
        invoice.TotalAmount = dto.TotalAmount;
        invoice.DocumentStatusId = dto.DocumentStatusId;
        invoice.Notes = dto.Notes?.Trim();
        invoice.CreatedByUserId = dto.CreatedByUserId;

        await _context.SaveChangesAsync();

        return Ok(new PurchaseInvoiceDto
        {
            PurchaseInvoiceId = invoice.PurchaseInvoiceId,
            InvoiceNumber = invoice.InvoiceNumber,
            InvoiceDate = invoice.InvoiceDate,
            SupplierId = invoice.SupplierId,
            WarehouseId = invoice.WarehouseId,
            CurrencyId = invoice.CurrencyId,
            ExchangeRate = invoice.ExchangeRate,
            SubTotal = invoice.SubTotal,
            DiscountAmount = invoice.DiscountAmount,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            DocumentStatusId = invoice.DocumentStatusId,
            Notes = invoice.Notes,
            CreatedAt = invoice.CreatedAt,
            CreatedByUserId = invoice.CreatedByUserId
        });
    }

    // DELETE: api/purchaseinvoices/1
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeletePurchaseInvoice(long id)
    {
        var invoice = await _context.PurchaseInvoices
            .FirstOrDefaultAsync(x => x.PurchaseInvoiceId == id);

        if (invoice == null)
        {
            return NotFound(new
            {
                message = "Purchase invoice not found."
            });
        }

        // Purchase Invoice does not have IsActive.
        // Use VOID status instead of physical delete.
        var voidStatus = await _context.DocumentStatuses
            .FirstOrDefaultAsync(x =>
                x.DocumentType == "PURCHASE" &&
                x.StatusCode == "VOID");

        if (voidStatus == null)
        {
            return BadRequest(new
            {
                message = "PURCHASE VOID status not found."
            });
        }

        invoice.DocumentStatusId = voidStatus.DocumentStatusId;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Purchase invoice voided successfully."
        });
    }
}