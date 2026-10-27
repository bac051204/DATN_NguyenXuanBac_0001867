using System.ComponentModel.DataAnnotations;

namespace SalesInventory.Api.Dtos;

public record CreateProductRequest(
    [Required, StringLength(50)] string Sku,
    [Required, StringLength(200)] string Name,
    [Range(0, double.MaxValue)] decimal Price,
    [Range(0, int.MaxValue)] int StockQuantity,
    int CategoryId);

public record UpdateProductRequest(
    [Required, StringLength(50)] string Sku,
    [Required, StringLength(200)] string Name,
    [Range(0, double.MaxValue)] decimal Price,
    [Range(0, int.MaxValue)] int StockQuantity,
    int CategoryId);

/// <summary>Điều chỉnh tồn kho: số dương = nhập kho, số âm = xuất kho.</summary>
public record AdjustStockRequest(int Delta);
