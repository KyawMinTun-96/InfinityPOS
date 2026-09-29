namespace InfinityPOS.API.DTOs;

public class ProductStockBatchBalanceDto
{
    public long ProductStockBatchId { get; set; }

    public int ProductId { get; set; }
    public int WarehouseId { get; set; }

    public long PurchaseInvoiceItemId { get; set; }

    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }

    public decimal OriginalQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }

    public int CurrencyId { get; set; }

    public DateTime CreatedAt { get; set; }
}