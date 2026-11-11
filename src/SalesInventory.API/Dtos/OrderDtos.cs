using System.ComponentModel.DataAnnotations;

namespace SalesInventory.Api.Dtos;

public record CreateOrderRequest(
    [Required, StringLength(200)] string CustomerName,
    [Required, MinLength(1)] List<OrderLineRequest> Items);

public record OrderLineRequest(
    int ProductId,
    [Range(1, int.MaxValue)] int Quantity);
