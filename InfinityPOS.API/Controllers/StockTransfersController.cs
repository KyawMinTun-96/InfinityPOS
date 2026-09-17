using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockTransfersController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public StockTransfersController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/stocktransfers
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockTransferDto>>> GetAll()
    {
        var transfers = await _context.StockTransfers
            .AsNoTracking()
            .OrderByDescending(x => x.StockTransferId)
            .Select(x => new StockTransferDto
            {
                StockTransferId = x.StockTransferId,
                TransferNumber = x.TransferNumber,
                TransferDate = x.TransferDate,
                FromWarehouseId = x.FromWarehouseId,
                ToWarehouseId = x.ToWarehouseId,
                DocumentStatusId = x.DocumentStatusId,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                CreatedByUserId = x.CreatedByUserId
            })
            .ToListAsync();

        return Ok(transfers);
    }

    // GET: api/stocktransfers/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<StockTransferDto>> GetById(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid StockTransferId."
            });
        }

        var transfer = await _context.StockTransfers
            .AsNoTracking()
            .Where(x => x.StockTransferId == id)
            .Select(x => new StockTransferDto
            {
                StockTransferId = x.StockTransferId,
                TransferNumber = x.TransferNumber,
                TransferDate = x.TransferDate,
                FromWarehouseId = x.FromWarehouseId,
                ToWarehouseId = x.ToWarehouseId,
                DocumentStatusId = x.DocumentStatusId,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                CreatedByUserId = x.CreatedByUserId
            })
            .FirstOrDefaultAsync();

        if (transfer == null)
        {
            return NotFound(new
            {
                message = "Stock transfer not found."
            });
        }

        return Ok(transfer);
    }

    // POST: api/stocktransfers
    [HttpPost]
    public async Task<ActionResult<StockTransferDto>> Create(
        CreateStockTransferDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TransferNumber))
        {
            return BadRequest(new
            {
                message = "TransferNumber is required."
            });
        }

        if (dto.TransferNumber.Length > 50)
        {
            return BadRequest(new
            {
                message = "TransferNumber must not exceed 50 characters."
            });
        }

        if (dto.FromWarehouseId <= 0 ||
            dto.ToWarehouseId <= 0)
        {
            return BadRequest(new
            {
                message = "Both warehouse IDs are required."
            });
        }

        if (dto.FromWarehouseId == dto.ToWarehouseId)
        {
            return BadRequest(new
            {
                message = "FromWarehouse and ToWarehouse cannot be the same."
            });
        }

        var fromWarehouse = await _context.Warehouses
            .FirstOrDefaultAsync(x =>
                x.WarehouseId == dto.FromWarehouseId &&
                x.IsActive);

        if (fromWarehouse == null)
        {
            return BadRequest(new
            {
                message = "FromWarehouse not found or inactive."
            });
        }

        var toWarehouse = await _context.Warehouses
            .FirstOrDefaultAsync(x =>
                x.WarehouseId == dto.ToWarehouseId &&
                x.IsActive);

        if (toWarehouse == null)
        {
            return BadRequest(new
            {
                message = "ToWarehouse not found or inactive."
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

        var duplicateNumber = await _context.StockTransfers
            .AnyAsync(x =>
                x.TransferNumber == dto.TransferNumber);

        if (duplicateNumber)
        {
            return Conflict(new
            {
                message = "TransferNumber already exists."
            });
        }

        // STOCK_TRANSFER DRAFT = 10
        const int draftStatusId = 10;

        var statusExists = await _context.DocumentStatuses
            .AnyAsync(x =>
                x.DocumentStatusId == draftStatusId &&
                x.StatusCode == "DRAFT");

        if (!statusExists)
        {
            return BadRequest(new
            {
                message = "STOCK_TRANSFER DRAFT document status not found."
            });
        }

        var transfer = new Data.Models.StockTransfer
        {
            TransferNumber = dto.TransferNumber.Trim(),
            TransferDate = dto.TransferDate ?? DateTime.Now,
            FromWarehouseId = dto.FromWarehouseId,
            ToWarehouseId = dto.ToWarehouseId,
            DocumentStatusId = draftStatusId,
            Notes = dto.Notes,
            CreatedAt = DateTime.Now,
            CreatedByUserId = dto.CreatedByUserId
        };

        _context.StockTransfers.Add(transfer);

        await _context.SaveChangesAsync();

        var result = new StockTransferDto
        {
            StockTransferId = transfer.StockTransferId,
            TransferNumber = transfer.TransferNumber,
            TransferDate = transfer.TransferDate,
            FromWarehouseId = transfer.FromWarehouseId,
            ToWarehouseId = transfer.ToWarehouseId,
            DocumentStatusId = transfer.DocumentStatusId,
            Notes = transfer.Notes,
            CreatedAt = transfer.CreatedAt,
            CreatedByUserId = transfer.CreatedByUserId
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = transfer.StockTransferId },
            result);
    }


    // POST: api/stocktransfers/1/post
    [HttpPost("{id:long}/post")]
    public async Task<ActionResult<StockTransferDto>> PostTransfer(long id)
    {
        if (id <= 0)
        {
            return BadRequest(new
            {
                message = "Invalid StockTransferId."
            });
        }

        var transfer = await _context.StockTransfers
            .FirstOrDefaultAsync(x => x.StockTransferId == id);

        if (transfer == null)
        {
            return NotFound(new
            {
                message = "Stock transfer not found."
            });
        }

        // Only DRAFT transfer can be posted.
        if (transfer.DocumentStatusId != 10)
        {
            return BadRequest(new
            {
                message = "Only DRAFT stock transfer can be posted."
            });
        }

        var items = await _context.StockTransferItems
            .Where(x => x.StockTransferId == id)
            .ToListAsync();

        if (items.Count == 0)
        {
            return BadRequest(new
            {
                message = "Stock transfer must contain at least one item."
            });
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in items)
            {
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        $"Invalid quantity for Product ID {item.ProductId}.");
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId &&
                        x.IsActive);

                if (product == null)
                {
                    throw new InvalidOperationException(
                        $"Product ID {item.ProductId} not found or inactive.");
                }

                // Get source stock
                var sourceBalance = await _context.StockBalances
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId &&
                        x.WarehouseId == transfer.FromWarehouseId);

                if (sourceBalance == null)
                {
                    throw new InvalidOperationException(
                        $"No stock balance found for Product ID {item.ProductId} " +
                        $"in source warehouse.");
                }

                if (sourceBalance.Quantity < item.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for Product ID {item.ProductId}. " +
                        $"Available: {sourceBalance.Quantity}, " +
                        $"Requested: {item.Quantity}.");
                }

                // Use source average cost as the actual transfer cost.
                var transferCost = sourceBalance.AverageCost;

                // Decrease source warehouse stock.
                sourceBalance.Quantity -= item.Quantity;
                sourceBalance.UpdatedAt = DateTime.Now;

                // Get destination stock
                var destinationBalance = await _context.StockBalances
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId &&
                        x.WarehouseId == transfer.ToWarehouseId);

                if (destinationBalance == null)
                {
                    destinationBalance = new Data.Models.StockBalance
                    {
                        ProductId = item.ProductId,
                        WarehouseId = transfer.ToWarehouseId,
                        Quantity = item.Quantity,
                        AverageCost = transferCost,
                        UpdatedAt = DateTime.Now
                    };

                    _context.StockBalances.Add(destinationBalance);
                }
                else
                {
                    var oldQuantity = destinationBalance.Quantity;
                    var oldAverageCost = destinationBalance.AverageCost;

                    var newQuantity = oldQuantity + item.Quantity;

                    if (newQuantity > 0)
                    {
                        destinationBalance.AverageCost =
                            ((oldQuantity * oldAverageCost) +
                            (item.Quantity * transferCost))
                            / newQuantity;
                    }

                    destinationBalance.Quantity = newQuantity;
                    destinationBalance.UpdatedAt = DateTime.Now;
                }

                // TRANSFER_OUT = 6
                var transferOut = new Data.Models.StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = transfer.FromWarehouseId,
                    StockMovementTypeId = 6,
                    Quantity = item.Quantity,
                    UnitCost = transferCost,
                    ReferenceType = "STOCK_TRANSFER",
                    ReferenceId = transfer.StockTransferId,
                    MovementDate = transfer.TransferDate,
                    Notes = $"Transfer OUT: {transfer.TransferNumber}",
                    CreatedByUserId = transfer.CreatedByUserId
                };

                // TRANSFER_IN = 5
                var transferIn = new Data.Models.StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = transfer.ToWarehouseId,
                    StockMovementTypeId = 5,
                    Quantity = item.Quantity,
                    UnitCost = transferCost,
                    ReferenceType = "STOCK_TRANSFER",
                    ReferenceId = transfer.StockTransferId,
                    MovementDate = transfer.TransferDate,
                    Notes = $"Transfer IN: {transfer.TransferNumber}",
                    CreatedByUserId = transfer.CreatedByUserId
                };

                _context.StockMovements.Add(transferOut);
                _context.StockMovements.Add(transferIn);
            }

            // STOCK_TRANSFER POSTED = 11
            transfer.DocumentStatusId = 11;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new StockTransferDto
            {
                StockTransferId = transfer.StockTransferId,
                TransferNumber = transfer.TransferNumber,
                TransferDate = transfer.TransferDate,
                FromWarehouseId = transfer.FromWarehouseId,
                ToWarehouseId = transfer.ToWarehouseId,
                DocumentStatusId = transfer.DocumentStatusId,
                Notes = transfer.Notes,
                CreatedAt = transfer.CreatedAt,
                CreatedByUserId = transfer.CreatedByUserId
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();

            return BadRequest(new
            {
                message = "Failed to post stock transfer.",
                error = ex.Message
            });
        }
    }
}