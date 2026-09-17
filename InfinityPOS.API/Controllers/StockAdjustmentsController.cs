using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockAdjustmentsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public StockAdjustmentsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/stockadjustments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockAdjustmentDto>>> GetStockAdjustments()
    {
        var adjustments = await _context.StockAdjustments
            .AsNoTracking()
            .OrderByDescending(x => x.AdjustmentDate)
            .Select(x => new StockAdjustmentDto
            {
                StockAdjustmentId = x.StockAdjustmentId,
                AdjustmentNumber = x.AdjustmentNumber,
                AdjustmentDate = x.AdjustmentDate,
                WarehouseId = x.WarehouseId,
                StockAdjustmentReasonId = x.StockAdjustmentReasonId,
                DocumentStatusId = x.DocumentStatusId,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                CreatedByUserId = x.CreatedByUserId
            })
            .ToListAsync();

        return Ok(adjustments);
    }

    // GET: api/stockadjustments/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<StockAdjustmentDto>> GetStockAdjustment(long id)
    {
        var adjustment = await _context.StockAdjustments
            .AsNoTracking()
            .Where(x => x.StockAdjustmentId == id)
            .Select(x => new StockAdjustmentDto
            {
                StockAdjustmentId = x.StockAdjustmentId,
                AdjustmentNumber = x.AdjustmentNumber,
                AdjustmentDate = x.AdjustmentDate,
                WarehouseId = x.WarehouseId,
                StockAdjustmentReasonId = x.StockAdjustmentReasonId,
                DocumentStatusId = x.DocumentStatusId,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                CreatedByUserId = x.CreatedByUserId
            })
            .FirstOrDefaultAsync();

        if (adjustment == null)
        {
            return NotFound(new
            {
                message = "Stock adjustment not found."
            });
        }

        return Ok(adjustment);
    }

    // POST: api/stockadjustments
    [HttpPost]
    public async Task<ActionResult<StockAdjustmentDto>> CreateStockAdjustment(
        CreateStockAdjustmentDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.AdjustmentNumber))
        {
            return BadRequest(new
            {
                message = "AdjustmentNumber is required."
            });
        }

        if (dto.WarehouseId <= 0)
        {
            return BadRequest(new
            {
                message = "WarehouseId must be greater than 0."
            });
        }

        if (dto.StockAdjustmentReasonId <= 0)
        {
            return BadRequest(new
            {
                message = "StockAdjustmentReasonId must be greater than 0."
            });
        }

        var duplicateNumber = await _context.StockAdjustments
            .AnyAsync(x =>
                x.AdjustmentNumber == dto.AdjustmentNumber);

        if (duplicateNumber)
        {
            return Conflict(new
            {
                message = "AdjustmentNumber already exists."
            });
        }

        var warehouse = await _context.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.WarehouseId == dto.WarehouseId &&
                x.IsActive);

        if (warehouse == null)
        {
            return BadRequest(new
            {
                message = "Warehouse not found or inactive."
            });
        }

        var reason = await _context.StockAdjustmentReasons
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.StockAdjustmentReasonId ==
                dto.StockAdjustmentReasonId &&
                x.IsActive);

        if (reason == null)
        {
            return BadRequest(new
            {
                message = "Stock adjustment reason not found or inactive."
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

        var adjustment = new Data.Models.StockAdjustment
        {
            AdjustmentNumber = dto.AdjustmentNumber.Trim(),
            AdjustmentDate = dto.AdjustmentDate ?? DateTime.Now,
            WarehouseId = dto.WarehouseId,
            StockAdjustmentReasonId = dto.StockAdjustmentReasonId,
            DocumentStatusId = 7,
            Notes = dto.Notes,
            CreatedAt = DateTime.Now,
            CreatedByUserId = dto.CreatedByUserId
        };

        _context.StockAdjustments.Add(adjustment);

        await _context.SaveChangesAsync();

        var result = new StockAdjustmentDto
        {
            StockAdjustmentId = adjustment.StockAdjustmentId,
            AdjustmentNumber = adjustment.AdjustmentNumber,
            AdjustmentDate = adjustment.AdjustmentDate,
            WarehouseId = adjustment.WarehouseId,
            StockAdjustmentReasonId = adjustment.StockAdjustmentReasonId,
            DocumentStatusId = adjustment.DocumentStatusId,
            Notes = adjustment.Notes,
            CreatedAt = adjustment.CreatedAt,
            CreatedByUserId = adjustment.CreatedByUserId
        };

        return CreatedAtAction(
            nameof(GetStockAdjustment),
            new { id = adjustment.StockAdjustmentId },
            result);
    }

    // POST: api/stockadjustments/1/post
    [HttpPost("{id:long}/post")]
    public async Task<ActionResult<StockAdjustmentDto>> PostStockAdjustment(long id)
    {
        var adjustment = await _context.StockAdjustments
            .FirstOrDefaultAsync(x => x.StockAdjustmentId == id);

        if (adjustment == null)
        {
            return NotFound(new
            {
                message = "Stock adjustment not found."
            });
        }

        // Only DRAFT adjustment can be posted.
        if (adjustment.DocumentStatusId != 7)
        {
            return BadRequest(new
            {
                message = "Only DRAFT stock adjustments can be posted."
            });
        }

        var items = await _context.StockAdjustmentItems
            .Where(x => x.StockAdjustmentId == id)
            .ToListAsync();

        if (items.Count == 0)
        {
            return BadRequest(new
            {
                message = "Stock adjustment must contain at least one item."
            });
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var item in items)
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId &&
                        x.IsActive);

                if (product == null)
                {
                    await transaction.RollbackAsync();

                    return BadRequest(new
                    {
                        message = $"Product {item.ProductId} not found or inactive."
                    });
                }

                var stockBalance = await _context.StockBalances
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == item.ProductId &&
                        x.WarehouseId == adjustment.WarehouseId);

                if (stockBalance == null)
                {
                    if (!item.IsIncrease)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(new
                        {
                            message = $"Insufficient stock for Product {item.ProductId}."
                        });
                    }

                    stockBalance = new Data.Models.StockBalance
                    {
                        ProductId = item.ProductId,
                        WarehouseId = adjustment.WarehouseId,
                        Quantity = 0,
                        AverageCost = 0,
                        UpdatedAt = DateTime.Now
                    };

                    _context.StockBalances.Add(stockBalance);
                }

                if (item.IsIncrease)
                {
                    var oldQuantity = stockBalance.Quantity;
                    var oldAverageCost = stockBalance.AverageCost;

                    var newQuantity = oldQuantity + item.Quantity;

                    if (newQuantity > 0)
                    {
                        stockBalance.AverageCost =
                            ((oldQuantity * oldAverageCost) +
                            (item.Quantity * item.UnitCost))
                            / newQuantity;
                    }

                    stockBalance.Quantity = newQuantity;
                }
                else
                {
                    if (stockBalance.Quantity < item.Quantity)
                    {
                        await transaction.RollbackAsync();

                        return BadRequest(new
                        {
                            message = $"Insufficient stock for Product {item.ProductId}.",
                            availableQuantity = stockBalance.Quantity,
                            requestedQuantity = item.Quantity
                        });
                    }

                    stockBalance.Quantity -= item.Quantity;
                }

                stockBalance.UpdatedAt = DateTime.Now;

                var movementTypeId = item.IsIncrease ? 3 : 4;

                var movement = new Data.Models.StockMovement
                {
                    ProductId = item.ProductId,
                    WarehouseId = adjustment.WarehouseId,
                    StockMovementTypeId = movementTypeId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    ReferenceType = "STOCK_ADJUSTMENT",
                    ReferenceId = adjustment.StockAdjustmentId,
                    MovementDate = adjustment.AdjustmentDate,
                    Notes = item.Notes ?? adjustment.Notes,
                    CreatedByUserId = adjustment.CreatedByUserId
                };

                _context.StockMovements.Add(movement);
            }

            // DRAFT -> POSTED
            adjustment.DocumentStatusId = 8;

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var result = new StockAdjustmentDto
            {
                StockAdjustmentId = adjustment.StockAdjustmentId,
                AdjustmentNumber = adjustment.AdjustmentNumber,
                AdjustmentDate = adjustment.AdjustmentDate,
                WarehouseId = adjustment.WarehouseId,
                StockAdjustmentReasonId = adjustment.StockAdjustmentReasonId,
                DocumentStatusId = adjustment.DocumentStatusId,
                Notes = adjustment.Notes,
                CreatedAt = adjustment.CreatedAt,
                CreatedByUserId = adjustment.CreatedByUserId
            };

            return Ok(result);
        }
        catch
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                message = "Failed to post stock adjustment."
            });
        }
    }
}