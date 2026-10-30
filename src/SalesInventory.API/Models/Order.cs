using System.Text.Json.Serialization;

namespace SalesInventory.Api.Models;

public enum OrderStatus
{
    Completed,
    Cancelled
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.UnitPrice * i.Quantity);
}

public class OrderItem
{
    public int Id { get; set; }

    [JsonIgnore]
    public int OrderId { get; set; }

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}
