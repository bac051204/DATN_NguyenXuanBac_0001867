using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SalesInventory.Api.Data;
using SalesInventory.Api.Dtos;
using SalesInventory.Api.Models;

namespace SalesInventory.Api.Tests;

public class ApiIntegrationTests : IDisposable
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    // Mỗi test có factory + SQLite in-memory riêng để dữ liệu không ảnh hưởng lẫn nhau.
    private readonly TestDb _db = new();
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.AddDbContext<AppDbContext>(o => o.UseSqlite(_db.Connection));
            }));
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        _db.Dispose();
    }

    [Fact]
    public async Task ProductCrud_FullFlow()
    {
        // POST
        var created = await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("SP001", "Bút bi", 5000, 10, 1));
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var product = (await created.Content.ReadFromJsonAsync<Product>(Json))!;

        // GET list + GET by id
        var all = await _client.GetFromJsonAsync<List<Product>>("/api/products", Json);
        Assert.Single(all!);
        Assert.Equal("Bút bi", (await GetProduct(product.Id)).Name);

        // PUT
        var updated = await _client.PutAsJsonAsync($"/api/products/{product.Id}",
            new UpdateProductRequest("SP001", "Bút bi xanh", 6000, 8, 2));
        updated.EnsureSuccessStatusCode();
        var afterUpdate = await GetProduct(product.Id);
        Assert.Equal((6000m, 2), (afterUpdate.Price, afterUpdate.CategoryId));

        // DELETE
        var deleted = await _client.DeleteAsync($"/api/products/{product.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/products/{product.Id}")).StatusCode);
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(1000, -1)]
    public async Task CreateProduct_NegativePriceOrStock_Returns400(decimal price, int stock)
    {
        var response = await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("SP001", "Bút bi", price, stock, 1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_NegativePrice_Returns400()
    {
        var product = await CreateProduct("SP001", stock: 1);

        var response = await _client.PutAsJsonAsync($"/api/products/{product.Id}",
            new UpdateProductRequest("SP001", "Bút bi", -5, 1, 1));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_DuplicateSku_Returns409()
    {
        await CreateProduct("SP001", stock: 1);

        var response = await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest("SP001", "Khác", 1, 1, 1));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task DeleteProduct_Missing_Returns404()
    {
        var response = await _client.DeleteAsync("/api/products/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task OrderFlow_CreateAndCancel_UpdatesStock()
    {
        var product = await CreateProduct("SP001", stock: 10);

        var orderResponse = await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest("Khách A", [new OrderLineRequest(product.Id, 4)]));
        Assert.Equal(HttpStatusCode.Created, orderResponse.StatusCode);
        var order = await orderResponse.Content.ReadFromJsonAsync<Order>(Json);
        Assert.Equal(6, (await GetProduct(product.Id)).StockQuantity);

        var cancel = await _client.PostAsync($"/api/orders/{order!.Id}/cancel", null);
        cancel.EnsureSuccessStatusCode();
        Assert.Equal(10, (await GetProduct(product.Id)).StockQuantity);
    }

    [Fact]
    public async Task CreateOrder_InsufficientStock_Returns409()
    {
        var product = await CreateProduct("SP001", stock: 1);

        var response = await _client.PostAsJsonAsync("/api/orders",
            new CreateOrderRequest("Khách A", [new OrderLineRequest(product.Id, 2)]));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private async Task<Product> CreateProduct(string sku, int stock)
    {
        var response = await _client.PostAsJsonAsync("/api/products",
            new CreateProductRequest(sku, $"Sản phẩm {sku}", 1000, stock, 1));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Product>(Json))!;
    }

    private async Task<Product> GetProduct(int id) =>
        (await _client.GetFromJsonAsync<Product>($"/api/products/{id}", Json))!;
}
