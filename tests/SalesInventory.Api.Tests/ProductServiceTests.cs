using SalesInventory.Api.Dtos;
using SalesInventory.Api.Services;

namespace SalesInventory.Api.Tests;

public class ProductServiceTests : IDisposable
{
    private const int CategoryId = 1;
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    private ProductService NewService() => new(_db.CreateContext());

    private Task<Domain.Entities.Product> Create(string sku, int stock = 10) =>
        NewService().CreateAsync(new CreateProductRequest(sku, $"Sản phẩm {sku}", 5000, stock, CategoryId));

    [Fact]
    public async Task Create_PersistsProduct()
    {
        var product = await Create("SP001");

        var fetched = await NewService().GetByIdAsync(product.Id);
        Assert.Equal("SP001", fetched.Sku);
        Assert.Equal(CategoryId, fetched.CategoryId);
    }

    [Fact]
    public async Task Create_DuplicateSkuIgnoringCase_Throws()
    {
        await Create("SP001");

        await Assert.ThrowsAsync<BusinessRuleException>(() => Create("sp001"));
    }

    [Fact]
    public async Task Create_UnknownCategory_Throws()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            NewService().CreateAsync(new CreateProductRequest("SP001", "Bút bi", 5000, 10, 999)));
    }

    [Fact]
    public async Task Update_ChangesAllFields()
    {
        var product = await Create("SP001");

        await NewService().UpdateAsync(product.Id, new UpdateProductRequest("SP001-X", "Bút mực", 7000, 3, 2));

        var fetched = await NewService().GetByIdAsync(product.Id);
        Assert.Equal(("SP001-X", "Bút mực", 7000m, 3, 2),
            (fetched.Sku, fetched.Name, fetched.SalePrice, fetched.StockQuantity, fetched.CategoryId));
    }

    [Fact]
    public async Task Update_SkuTakenByAnotherProduct_Throws()
    {
        await Create("SP001");
        var other = await Create("SP002");

        await Assert.ThrowsAsync<BusinessRuleException>(() =>
            NewService().UpdateAsync(other.Id, new UpdateProductRequest("SP001", "X", 1, 1, CategoryId)));
    }

    [Fact]
    public async Task Delete_RemovesProduct()
    {
        var product = await Create("SP001");

        await NewService().DeleteAsync(product.Id);

        await Assert.ThrowsAsync<NotFoundException>(() => NewService().GetByIdAsync(product.Id));
    }

    [Fact]
    public async Task Delete_ProductInOrder_Throws()
    {
        var product = await Create("SP001");
        await new OrderService(_db.CreateContext(), TimeProvider.System)
            .CreateAsync(new CreateOrderRequest("Khách A", [new OrderLineRequest(product.Id, 1)]));

        await Assert.ThrowsAsync<BusinessRuleException>(() => NewService().DeleteAsync(product.Id));
    }

    [Fact]
    public async Task AdjustStock_BelowZero_ThrowsAndKeepsQuantity()
    {
        var product = await Create("SP001", stock: 2);

        await Assert.ThrowsAsync<BusinessRuleException>(() => NewService().AdjustStockAsync(product.Id, -3));
        Assert.Equal(2, (await NewService().GetByIdAsync(product.Id)).StockQuantity);
    }

    [Fact]
    public async Task GetLowStock_ReturnsProductsAtOrBelowThreshold()
    {
        await Create("A", stock: 2);
        await Create("B", stock: 5);
        await Create("C", stock: 50);

        var low = await NewService().GetLowStockAsync(5);

        Assert.Equal(new[] { "A", "B" }, low.Select(p => p.Sku));
    }
}
