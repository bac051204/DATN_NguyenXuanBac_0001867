namespace SalesInventory.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }

    // Current on-hand quantity (cache). StockMovement is the source of truth.
    // Configured as a concurrency token in Infrastructure so two concurrent
    // orders cannot overwrite each other's stock deduction.
    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}
