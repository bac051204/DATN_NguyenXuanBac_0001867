using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SalesInventory.Api.Models;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }

    // Concurrency token: hai đơn hàng cùng trừ kho một sản phẩm sẽ không ghi đè lẫn nhau.
    [ConcurrencyCheck]
    public int StockQuantity { get; set; }

    public int CategoryId { get; set; }

    [JsonIgnore]
    public Category? Category { get; set; }
}
