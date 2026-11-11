using Microsoft.AspNetCore.Mvc;
using SalesInventory.Api.Dtos;
using SalesInventory.Domain.Entities;
using SalesInventory.Api.Services;

namespace SalesInventory.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrdersController(OrderService orders) : ControllerBase
{
    [HttpGet]
    public Task<List<Order>> GetAll() => orders.GetAllAsync();

    [HttpGet("{id:int}")]
    public Task<Order> GetById(int id) => orders.GetByIdAsync(id);

    [HttpPost]
    public async Task<ActionResult<Order>> Create(CreateOrderRequest request)
    {
        var order = await orders.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    [HttpPost("{id:int}/cancel")]
    public Task<Order> Cancel(int id) => orders.CancelAsync(id);
}
