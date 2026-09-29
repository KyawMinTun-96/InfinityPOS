using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace InfinityPOS.API.Controllers;

[Authorize]
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
            .AsNoTracking()
            .OrderByDescending(x => x.PurchaseInvoiceId)
            .Select(x => new PurchaseInvoiceDto
            {
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                InvoiceNumber = x.InvoiceNumber,
                InvoiceDate = x.InvoiceDate,

                SupplierId = x.SupplierId,
                SupplierName = x.SupplierId.HasValue
                    ? _context.Suppliers
                        .Where(s => s.SupplierId == x.SupplierId.Value)
                        .Select(s => s.SupplierName)
                        .FirstOrDefault()
                    : null,

                WarehouseId = x.WarehouseId,
                WarehouseName = _context.Warehouses
                    .Where(w => w.WarehouseId == x.WarehouseId)
                    .Select(w => w.WarehouseName)
                    .FirstOrDefault(),

                CurrencyId = x.CurrencyId,
                CurrencyCode = _context.Currencies
                    .Where(c => c.CurrencyId == x.CurrencyId)
                    .Select(c => c.CurrencyCode)
                    .FirstOrDefault(),

                CurrencyName = _context.Currencies
                    .Where(c => c.CurrencyId == x.CurrencyId)
                    .Select(c => c.CurrencyName)
                    .FirstOrDefault(),

                ExchangeRate = x.ExchangeRate,
                SubTotal = x.SubTotal,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount,

                DocumentStatusId = x.DocumentStatusId,

                StatusCode = _context.DocumentStatuses
                    .Where(s =>
                        s.DocumentStatusId == x.DocumentStatusId)
                    .Select(s => s.StatusCode)
                    .FirstOrDefault(),

                StatusName = _context.DocumentStatuses
                    .Where(s =>
                        s.DocumentStatusId == x.DocumentStatusId)
                    .Select(s => s.StatusName)
                    .FirstOrDefault(),

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
            .AsNoTracking()
            .Where(x => x.PurchaseInvoiceId == id)
            .Select(x => new PurchaseInvoiceDto
            {
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                InvoiceNumber = x.InvoiceNumber,
                InvoiceDate = x.InvoiceDate,

                SupplierId = x.SupplierId,
                SupplierName = x.SupplierId.HasValue
                    ? _context.Suppliers
                        .Where(s => s.SupplierId == x.SupplierId.Value)
                        .Select(s => s.SupplierName)
                        .FirstOrDefault()
                    : null,

                WarehouseId = x.WarehouseId,
                WarehouseName = _context.Warehouses
                    .Where(w => w.WarehouseId == x.WarehouseId)
                    .Select(w => w.WarehouseName)
                    .FirstOrDefault(),

                CurrencyId = x.CurrencyId,
                CurrencyCode = _context.Currencies
                    .Where(c => c.CurrencyId == x.CurrencyId)
                    .Select(c => c.CurrencyCode)
                    .FirstOrDefault(),

                CurrencyName = _context.Currencies
                    .Where(c => c.CurrencyId == x.CurrencyId)
                    .Select(c => c.CurrencyName)
                    .FirstOrDefault(),

                ExchangeRate = x.ExchangeRate,
                SubTotal = x.SubTotal,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount,

                DocumentStatusId = x.DocumentStatusId,

                StatusCode = _context.DocumentStatuses
                    .Where(s =>
                        s.DocumentStatusId == x.DocumentStatusId)
                    .Select(s => s.StatusCode)
                    .FirstOrDefault(),

                StatusName = _context.DocumentStatuses
                    .Where(s =>
                        s.DocumentStatusId == x.DocumentStatusId)
                    .Select(s => s.StatusName)
                    .FirstOrDefault(),

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

        var result = await _context.PurchaseInvoices
            .AsNoTracking()
            .Where(x =>
                x.PurchaseInvoiceId == invoice.PurchaseInvoiceId)
            .Select(x => new PurchaseInvoiceDto
            {
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                InvoiceNumber = x.InvoiceNumber,
                InvoiceDate = x.InvoiceDate,

                SupplierId = x.SupplierId,
                SupplierName = x.SupplierId.HasValue
                    ? _context.Suppliers
                        .Where(s => s.SupplierId == x.SupplierId.Value)
                        .Select(s => s.SupplierName)
                        .FirstOrDefault()
                    : null,

                WarehouseId = x.WarehouseId,
                WarehouseName = _context.Warehouses
                    .Where(w => w.WarehouseId == x.WarehouseId)
                    .Select(w => w.WarehouseName)
                    .FirstOrDefault(),

                CurrencyId = x.CurrencyId,
                CurrencyCode = _context.Currencies
                    .Where(c => c.CurrencyId == x.CurrencyId)
                    .Select(c => c.CurrencyCode)
                    .FirstOrDefault(),

                CurrencyName = _context.Currencies
                    .Where(c => c.CurrencyId == x.CurrencyId)
                    .Select(c => c.CurrencyName)
                    .FirstOrDefault(),

                ExchangeRate = x.ExchangeRate,
                SubTotal = x.SubTotal,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount,

                DocumentStatusId = x.DocumentStatusId,

                StatusCode = _context.DocumentStatuses
                    .Where(s =>
                        s.DocumentStatusId == x.DocumentStatusId)
                    .Select(s => s.StatusCode)
                    .FirstOrDefault(),

                StatusName = _context.DocumentStatuses
                    .Where(s =>
                        s.DocumentStatusId == x.DocumentStatusId)
                    .Select(s => s.StatusName)
                    .FirstOrDefault(),

                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                CreatedByUserId = x.CreatedByUserId
            })
            .FirstAsync();

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
                message =
                    "WarehouseId, CurrencyId and DocumentStatusId are required."
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
            .FirstOrDefaultAsync(x =>
                x.PurchaseInvoiceId == id);

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
                    message =
                        "CreatedByUserId not found or inactive."
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

        var result = await _context.PurchaseInvoices
            .AsNoTracking()
            .Where(x =>
                x.PurchaseInvoiceId == invoice.PurchaseInvoiceId)
            .Select(x => new PurchaseInvoiceDto
            {
                PurchaseInvoiceId = x.PurchaseInvoiceId,
                InvoiceNumber = x.InvoiceNumber,
                InvoiceDate = x.InvoiceDate,

                SupplierId = x.SupplierId,
                SupplierName = x.SupplierId.HasValue
                    ? _context.Suppliers
                        .Where(s => s.SupplierId == x.SupplierId.Value)
                        .Select(s => s.SupplierName)
                        .FirstOrDefault()
                    : null,

                WarehouseId = x.WarehouseId,
                WarehouseName = _context.Warehouses
                    .Where(w => w.WarehouseId == x.WarehouseId)
                    .Select(w => w.WarehouseName)
                    .FirstOrDefault(),

                CurrencyId = x.CurrencyId,
                CurrencyCode = _context.Currencies
                    .Where(c => c.CurrencyId == x.CurrencyId)
                    .Select(c => c.CurrencyCode)
                    .FirstOrDefault(),

                CurrencyName = _context.Currencies
                    .Where(c => c.CurrencyId == x.CurrencyId)
                    .Select(c => c.CurrencyName)
                    .FirstOrDefault(),

                ExchangeRate = x.ExchangeRate,
                SubTotal = x.SubTotal,
                DiscountAmount = x.DiscountAmount,
                TaxAmount = x.TaxAmount,
                TotalAmount = x.TotalAmount,

                DocumentStatusId = x.DocumentStatusId,

                StatusCode = _context.DocumentStatuses
                    .Where(s =>
                        s.DocumentStatusId == x.DocumentStatusId)
                    .Select(s => s.StatusCode)
                    .FirstOrDefault(),

                StatusName = _context.DocumentStatuses
                    .Where(s =>
                        s.DocumentStatusId == x.DocumentStatusId)
                    .Select(s => s.StatusName)
                    .FirstOrDefault(),

                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                CreatedByUserId = x.CreatedByUserId
            })
            .FirstAsync();

        return Ok(result);
    }

    // POST: api/purchaseinvoices/1/post
    [HttpPost("{id:long}/post")]
    public async Task<IActionResult> PostPurchaseInvoice(long id)
    {
        var invoice = await _context.PurchaseInvoices
            .FirstOrDefaultAsync(x =>
                x.PurchaseInvoiceId == id);

        if (invoice == null)
        {
            return NotFound(new
            {
                message = "Purchase invoice not found."
            });
        }

        // Get Purchase document statuses
        var draftStatus = await _context.DocumentStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.DocumentType == "PURCHASE" &&
                x.StatusCode == "DRAFT");

        var postedStatus = await _context.DocumentStatuses
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.DocumentType == "PURCHASE" &&
                x.StatusCode == "POSTED");

        if (draftStatus == null ||
            postedStatus == null)
        {
            return BadRequest(new
            {
                message =
                    "Purchase DRAFT or POSTED status not found."
            });
        }

        // Only DRAFT invoices can be posted
        if (invoice.DocumentStatusId !=
            draftStatus.DocumentStatusId)
        {
            return BadRequest(new
            {
                message =
                    "Only DRAFT purchase invoices can be posted."
            });
        }

        // Load invoice items
        var items = await _context.PurchaseInvoiceItems
            .Where(x =>
                x.PurchaseInvoiceId == id)
            .ToListAsync();

        if (items.Count == 0)
        {
            return BadRequest(new
            {
                message =
                    "Cannot post purchase invoice without items."
            });
        }

        // Prevent duplicate stock posting
        var existingMovement = await _context.StockMovements
            .AsNoTracking()
            .AnyAsync(x =>
                x.ReferenceType == "PURCHASE_INVOICE" &&
                x.ReferenceId == id);

        if (existingMovement)
        {
            return BadRequest(new
            {
                message =
                    "Stock movement already exists for this purchase invoice."
            });
        }

        // Prevent duplicate stock batch posting
        var existingBatch = await _context.ProductStockBatches
            .AsNoTracking()
            .AnyAsync(x =>
                x.PurchaseInvoiceItem.PurchaseInvoiceId == id);

        if (existingBatch)
        {
            return BadRequest(new
            {
                message =
                    "Stock batch already exists for this purchase invoice."
            });
        }

        // Validate warehouse
        var warehouse = await _context.Warehouses
            .FirstOrDefaultAsync(x =>
                x.WarehouseId == invoice.WarehouseId &&
                x.IsActive);

        if (warehouse == null)
        {
            return BadRequest(new
            {
                message =
                    "Warehouse not found or inactive."
            });
        }

        // Validate currency
        var currency = await _context.Currencies
            .FirstOrDefaultAsync(x =>
                x.CurrencyId == invoice.CurrencyId &&
                x.IsActive);

        if (currency == null)
        {
            return BadRequest(new
            {
                message =
                    "Currency not found or inactive."
            });
        }

        // Get PURCHASE_IN movement type
        var purchaseInType = await _context.StockMovementTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.MovementCode == "PURCHASE_IN" &&
                x.Direction == "IN");

        if (purchaseInType == null)
        {
            return BadRequest(new
            {
                message =
                    "PURCHASE_IN stock movement type not found."
            });
        }

        // Validate all products before changing stock
        foreach (var item in items)
        {
            if (item.Quantity <= 0)
            {
                return BadRequest(new
                {
                    message =
                        $"Invalid quantity for PurchaseInvoiceItemId {item.PurchaseInvoiceItemId}."
                });
            }

            if (item.UnitCost < 0)
            {
                return BadRequest(new
                {
                    message =
                        $"Invalid unit cost for PurchaseInvoiceItemId {item.PurchaseInvoiceItemId}."
                });
            }

            if (item.SalePrice < 0)
            {
                return BadRequest(new
                {
                    message =
                        $"Invalid sale price for PurchaseInvoiceItemId {item.PurchaseInvoiceItemId}."
                });
            }

            var productExists = await _context.Products
                .AnyAsync(x =>
                    x.ProductId == item.ProductId &&
                    x.IsActive);

            if (!productExists)
            {
                return BadRequest(new
                {
                    message =
                        $"Product ID {item.ProductId} not found or inactive."
                });
            }
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in items)
            {
                var stockBalance = await _context.StockBalances
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId &&
                        x.WarehouseId == invoice.WarehouseId);

                if (stockBalance == null)
                {
                    stockBalance = new Data.Models.StockBalance
                    {
                        ProductId = item.ProductId,
                        WarehouseId = invoice.WarehouseId,
                        Quantity = 0,
                        AverageCost = 0,
                        UpdatedAt = DateTime.Now
                    };

                    _context.StockBalances.Add(stockBalance);
                }

                var oldQuantity = stockBalance.Quantity;
                var oldAverageCost = stockBalance.AverageCost;

                var newQuantity =
                    oldQuantity + item.Quantity;

                if (newQuantity > 0)
                {
                    stockBalance.AverageCost =
                        (
                            (oldQuantity * oldAverageCost) +
                            (item.Quantity * item.UnitCost)
                        ) / newQuantity;
                }

                stockBalance.Quantity = newQuantity;
                stockBalance.UpdatedAt = DateTime.Now;

                // Create stock movement
                var movement = new Data.Models.StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = invoice.WarehouseId,
                    StockMovementTypeId =
                        purchaseInType.StockMovementTypeId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    ReferenceType = "PURCHASE_INVOICE",
                    ReferenceId = invoice.PurchaseInvoiceId,
                    MovementDate = invoice.InvoiceDate,
                    Notes =
                        $"Purchase Invoice {invoice.InvoiceNumber}",
                    CreatedByUserId =
                        invoice.CreatedByUserId
                };

                _context.StockMovements.Add(movement);

                // Create stock batch
                var stockBatch = new Data.Models.ProductStockBatch
                {
                    ProductId = item.ProductId,
                    WarehouseId = invoice.WarehouseId,
                    PurchaseInvoiceItemId =
                        item.PurchaseInvoiceItemId,
                    CostPrice = item.UnitCost,
                    SalePrice = item.SalePrice,
                    OriginalQuantity = item.Quantity,
                    RemainingQuantity = item.Quantity,
                    CurrencyId = invoice.CurrencyId,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _context.ProductStockBatches.Add(stockBatch);
            }

            // Change invoice status only after stock processing
            invoice.DocumentStatusId =
                postedStatus.DocumentStatusId;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return Ok(new
            {
                message =
                    "Purchase invoice posted successfully.",
                purchaseInvoiceId =
                    invoice.PurchaseInvoiceId,
                invoiceNumber =
                    invoice.InvoiceNumber,
                documentStatusId =
                    invoice.DocumentStatusId,
                statusCode =
                    "POSTED",
                stockMovementType =
                    "PURCHASE_IN",
                stockBatchCreated =
                    true,
                itemCount =
                    items.Count
            });
        }
        catch
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                message =
                    "Failed to post purchase invoice. No stock changes were saved."
            });
        }
    }

    // DELETE: api/purchaseinvoices/1
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeletePurchaseInvoice(long id)
    {
        var invoice = await _context.PurchaseInvoices
            .FirstOrDefaultAsync(x =>
                x.PurchaseInvoiceId == id);

        if (invoice == null)
        {
            return NotFound(new
            {
                message = "Purchase invoice not found."
            });
        }

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

        invoice.DocumentStatusId =
            voidStatus.DocumentStatusId;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message =
                "Purchase invoice voided successfully."
        });
    }
}