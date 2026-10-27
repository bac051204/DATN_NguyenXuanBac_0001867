using Microsoft.AspNetCore.Mvc;
using SalesInventory.Api.Dtos;
using SalesInventory.Api.Models;
using SalesInventory.Api.Services;

namespace SalesInventory.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(ProductService products) : ControllerBase
{
    [HttpGet]
    public Task<List<Product>> GetAll() => products.GetAllAsync();

    [HttpGet("{id:int}")]
    public Task<Product> GetById(int id) => products.GetByIdAsync(id);

    [HttpGet("low-stock")]
    public Task<List<Product>> GetLowStock([FromQuery] int threshold = 5) => products.GetLowStockAsync(threshold);

    [HttpPost]
    public async Task<ActionResult<Product>> Create(CreateProductRequest request)
    {
        var product = await products.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    public Task<Product> Update(int id, UpdateProductRequest request) => products.UpdateAsync(id, request);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await products.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/stock")]
    public Task<Product> AdjustStock(int id, AdjustStockRequest request) => products.AdjustStockAsync(id, request.Delta);
}
