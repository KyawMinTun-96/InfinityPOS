using InfinityPOS.API.Data;
using InfinityPOS.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockMovementsController : ControllerBase
{
    private readonly InfinityPosDbContext _context;

    public StockMovementsController(InfinityPosDbContext context)
    {
        _context = context;
    }

    // GET: api/stockmovements
    [HttpGet]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetStockMovements()
    {
        var movements = await _context.StockMovements
            .AsNoTracking()
            .OrderByDescending(x => x.MovementDate)
            .Select(x => new StockMovementDto
            {
                StockMovementId = x.StockMovementId,
                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,
                StockMovementTypeId = x.StockMovementTypeId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                MovementDate = x.MovementDate,
                Notes = x.Notes,
                CreatedByUserId = x.CreatedByUserId
            })
            .ToListAsync();

        return Ok(movements);
    }

    // GET: api/stockmovements/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<StockMovementDto>> GetStockMovement(long id)
    {
        var movement = await _context.StockMovements
            .AsNoTracking()
            .Where(x => x.StockMovementId == id)
            .Select(x => new StockMovementDto
            {
                StockMovementId = x.StockMovementId,
                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,
                StockMovementTypeId = x.StockMovementTypeId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                MovementDate = x.MovementDate,
                Notes = x.Notes,
                CreatedByUserId = x.CreatedByUserId
            })
            .FirstOrDefaultAsync();

        if (movement == null)
        {
            return NotFound(new
            {
                message = "Stock movement not found."
            });
        }

        return Ok(movement);
    }

    // GET: api/stockmovements/product/1
    [HttpGet("product/{productId:int}")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetByProduct(
        int productId)
    {
        var productExists = await _context.Products
            .AnyAsync(x => x.ProductId == productId);

        if (!productExists)
        {
            return NotFound(new
            {
                message = "Product not found."
            });
        }

        var movements = await _context.StockMovements
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .OrderByDescending(x => x.MovementDate)
            .Select(x => new StockMovementDto
            {
                StockMovementId = x.StockMovementId,
                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,
                StockMovementTypeId = x.StockMovementTypeId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                MovementDate = x.MovementDate,
                Notes = x.Notes,
                CreatedByUserId = x.CreatedByUserId
            })
            .ToListAsync();

        return Ok(movements);
    }

    // GET: api/stockmovements/warehouse/1
    [HttpGet("warehouse/{warehouseId:int}")]
    public async Task<ActionResult<IEnumerable<StockMovementDto>>> GetByWarehouse(
        int warehouseId)
    {
        var warehouseExists = await _context.Warehouses
            .AnyAsync(x => x.WarehouseId == warehouseId);

        if (!warehouseExists)
        {
            return NotFound(new
            {
                message = "Warehouse not found."
            });
        }

        var movements = await _context.StockMovements
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId)
            .OrderByDescending(x => x.MovementDate)
            .Select(x => new StockMovementDto
            {
                StockMovementId = x.StockMovementId,
                ProductId = x.ProductId,
                WarehouseId = x.WarehouseId,
                StockMovementTypeId = x.StockMovementTypeId,
                Quantity = x.Quantity,
                UnitCost = x.UnitCost,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                MovementDate = x.MovementDate,
                Notes = x.Notes,
                CreatedByUserId = x.CreatedByUserId
            })
            .ToListAsync();

        return Ok(movements);
    }

    // POST: api/stockmovements
    [HttpPost]
    public async Task<ActionResult<StockMovementDto>> CreateStockMovement(
        CreateStockMovementDto dto)
    {
        if (dto.ProductId <= 0)
        {
            return BadRequest(new
            {
                message = "ProductId must be greater than 0."
            });
        }

        if (dto.WarehouseId <= 0)
        {
            return BadRequest(new
            {
                message = "WarehouseId must be greater than 0."
            });
        }

        if (dto.StockMovementTypeId <= 0)
        {
            return BadRequest(new
            {
                message = "StockMovementTypeId must be greater than 0."
            });
        }

        if (dto.Quantity <= 0)
        {
            return BadRequest(new
            {
                message = "Quantity must be greater than 0."
            });
        }

        if (dto.UnitCost < 0)
        {
            return BadRequest(new
            {
                message = "UnitCost cannot be negative."
            });
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.ProductId == dto.ProductId &&
                x.IsActive);

        if (product == null)
        {
            return BadRequest(new
            {
                message = "Product not found or inactive."
            });
        }

        var warehouse = await _context.Warehouses
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

        var movementType = await _context.StockMovementTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.StockMovementTypeId == dto.StockMovementTypeId);

        if (movementType == null)
        {
            return BadRequest(new
            {
                message = "Stock movement type not found."
            });
        }

        var direction = movementType.Direction.Trim().ToUpperInvariant();
        if (direction != "IN" &&
            direction != "OUT")
        {
            return BadRequest(new
            {
                message = "Invalid stock movement direction."
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

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var stockBalance = await _context.StockBalances
                .FirstOrDefaultAsync(x =>
                    x.ProductId == dto.ProductId &&
                    x.WarehouseId == dto.WarehouseId);

            if (stockBalance == null)
            {
                if (direction == "OUT")
                {
                    return BadRequest(new
                    {
                        message = "Insufficient stock. No stock balance exists."
                    });
                }

                stockBalance = new Data.Models.StockBalance
                {
                    ProductId = dto.ProductId,
                    WarehouseId = dto.WarehouseId,
                    Quantity = 0,
                    AverageCost = 0,
                    UpdatedAt = DateTime.Now
                };

                _context.StockBalances.Add(stockBalance);
            }

            if (direction == "OUT")
            {
                if (stockBalance.Quantity < dto.Quantity)
                {
                    return BadRequest(new
                    {
                        message = "Insufficient stock.",
                        availableQuantity = stockBalance.Quantity,
                        requestedQuantity = dto.Quantity
                    });
                }

                stockBalance.Quantity -= dto.Quantity;
            }
            else
            {
                var oldQuantity = stockBalance.Quantity;
                var oldAverageCost = stockBalance.AverageCost;

                var newQuantity = oldQuantity + dto.Quantity;

                if (newQuantity > 0)
                {
                    stockBalance.AverageCost =
                        ((oldQuantity * oldAverageCost) +
                         (dto.Quantity * dto.UnitCost))
                        / newQuantity;
                }

                stockBalance.Quantity = newQuantity;
            }

            stockBalance.UpdatedAt = DateTime.Now;

            var movement = new Data.Models.StockMovement
            {
                ProductId = dto.ProductId,
                WarehouseId = dto.WarehouseId,
                StockMovementTypeId = dto.StockMovementTypeId,
                Quantity = dto.Quantity,
                UnitCost = dto.UnitCost,
                ReferenceType = dto.ReferenceType,
                ReferenceId = dto.ReferenceId,
                MovementDate = dto.MovementDate ?? DateTime.Now,
                Notes = dto.Notes,
                CreatedByUserId = dto.CreatedByUserId
            };

            _context.StockMovements.Add(movement);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var result = new StockMovementDto
            {
                StockMovementId = movement.StockMovementId,
                ProductId = movement.ProductId,
                WarehouseId = movement.WarehouseId,
                StockMovementTypeId = movement.StockMovementTypeId,
                Quantity = movement.Quantity,
                UnitCost = movement.UnitCost,
                ReferenceType = movement.ReferenceType,
                ReferenceId = movement.ReferenceId,
                MovementDate = movement.MovementDate,
                Notes = movement.Notes,
                CreatedByUserId = movement.CreatedByUserId
            };

            return CreatedAtAction(
                nameof(GetStockMovement),
                new { id = movement.StockMovementId },
                result);
        }
        catch
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                message = "Failed to create stock movement."
            });
        }
    }
}