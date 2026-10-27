using SalesInventory.Api.Dtos;
using SalesInventory.Api.Models;
using SalesInventory.Api.Services;

namespace SalesInventory.Api.Tests;

public class OrderServiceTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private OrderService NewOrders() => new(_db.CreateContext(), TimeProvider.System);

    private Task<Product> CreateProduct(string sku, decimal price, int stock) =>
        new ProductService(_db.CreateContext()).CreateAsync(new CreateProductRequest(sku, sku, price, stock, 1));

    private async Task<int> Stock(int productId) =>
        (await new ProductService(_db.CreateContext()).GetByIdAsync(productId)).StockQuantity;

    [Fact]
    public async Task Create_DeductsStockAndCalculatesTotal()
    {
        var pen = await CreateProduct("SP001", 5000, 10);
        var book = await CreateProduct("SP002", 12000, 5);

        var order = await NewOrders().CreateAsync(new CreateOrderRequest("Khách A",
        [
            new OrderLineRequest(pen.Id, 3),
            new OrderLineRequest(book.Id, 2)
        ]));

        Assert.Equal(OrderStatus.Completed, order.Status);
        Assert.Equal(3 * 5000m + 2 * 12000m, (await NewOrders().GetByIdAsync(order.Id)).Total);
        Assert.Equal(7, await Stock(pen.Id));
        Assert.Equal(3, await Stock(book.Id));
    }

    [Fact]
    public async Task Create_DuplicateLines_AreMergedForStockCheck()
    {
        var pen = await CreateProduct("SP001", 5000, 5);

        await Assert.ThrowsAsync<BusinessRuleException>(() => NewOrders().CreateAsync(new CreateOrderRequest("Khách A",
        [
            new OrderLineRequest(pen.Id, 3),
            new OrderLineRequest(pen.Id, 3)
        ])));
        Assert.Equal(5, await Stock(pen.Id));
    }

    [Fact]
    public async Task Create_InsufficientStock_DoesNotChangeAnyStock()
    {
        var pen = await CreateProduct("SP001", 5000, 10);
        var book = await CreateProduct("SP002", 12000, 1);

        await Assert.ThrowsAsync<BusinessRuleException>(() => NewOrders().CreateAsync(new CreateOrderRequest("Khách A",
        [
            new OrderLineRequest(pen.Id, 3),
            new OrderLineRequest(book.Id, 2)
        ])));

        Assert.Equal(10, await Stock(pen.Id));
        Assert.Equal(1, await Stock(book.Id));
        Assert.Empty(await NewOrders().GetAllAsync());
    }

    [Fact]
    public async Task Create_UnknownProduct_Throws()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            NewOrders().CreateAsync(new CreateOrderRequest("Khách A", [new OrderLineRequest(42, 1)])));
    }

    [Fact]
    public async Task Cancel_RestoresStock_AndCannotCancelTwice()
    {
        var pen = await CreateProduct("SP001", 5000, 10);
        var order = await NewOrders().CreateAsync(new CreateOrderRequest("Khách A", [new OrderLineRequest(pen.Id, 4)]));

        var cancelled = await NewOrders().CancelAsync(order.Id);

        Assert.Equal(OrderStatus.Cancelled, cancelled.Status);
        Assert.Equal(10, await Stock(pen.Id));
        await Assert.ThrowsAsync<BusinessRuleException>(() => NewOrders().CancelAsync(order.Id));
    }
}
