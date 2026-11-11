using System.ComponentModel.DataAnnotations;

namespace SalesInventory.Api.Dtos;

public record CreateProductRequest(
    [Required, StringLength(50)] string Sku,
    [Required, StringLength(200)] string Name,
    [Range(0, double.MaxValue)] decimal SalePrice,
    [Range(0, int.MaxValue)] int StockQuantity,
    int CategoryId,
    [Range(0, double.MaxValue)] decimal PurchasePrice = 0);

public record UpdateProductRequest(
    [Required, StringLength(50)] string Sku,
    [Required, StringLength(200)] string Name,
    [Range(0, double.MaxValue)] decimal SalePrice,
    [Range(0, int.MaxValue)] int StockQuantity,
    int CategoryId,
    [Range(0, double.MaxValue)] decimal PurchasePrice = 0);

/// <summary>Điều chỉnh tồn kho: số dương = nhập kho, số âm = xuất kho.</summary>
public record AdjustStockRequest(int Delta);
