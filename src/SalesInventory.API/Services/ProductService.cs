using Microsoft.EntityFrameworkCore;
using SalesInventory.Infrastructure.Persistence;
using SalesInventory.Api.Dtos;
using SalesInventory.Domain.Entities;

namespace SalesInventory.Api.Services;

public class ProductService(AppDbContext db)
{
    public async Task<List<Product>> GetAllAsync() =>
        await db.Products.AsNoTracking().OrderBy(p => p.Id).ToListAsync();

    public async Task<Product> GetByIdAsync(int id) =>
        await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id)
        ?? throw new NotFoundException($"Không tìm thấy sản phẩm {id}.");

    public async Task<List<Product>> GetLowStockAsync(int threshold) =>
        await db.Products.AsNoTracking()
            .Where(p => p.StockQuantity <= threshold)
            .OrderBy(p => p.Id)
            .ToListAsync();

    public async Task<Product> CreateAsync(CreateProductRequest request)
    {
        await EnsureSkuAvailableAsync(request.Sku, excludeId: null);
        await EnsureCategoryExistsAsync(request.CategoryId);

        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            PurchasePrice = request.PurchasePrice,
            SalePrice = request.SalePrice,
            StockQuantity = request.StockQuantity,
            CategoryId = request.CategoryId
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await FindAsync(id);
        await EnsureSkuAvailableAsync(request.Sku, excludeId: id);
        await EnsureCategoryExistsAsync(request.CategoryId);

        product.Sku = request.Sku;
        product.Name = request.Name;
        product.PurchasePrice = request.PurchasePrice;
        product.SalePrice = request.SalePrice;
        product.StockQuantity = request.StockQuantity;
        product.CategoryId = request.CategoryId;
        await db.SaveChangesAsync();
        return product;
    }

    public async Task DeleteAsync(int id)
    {
        var product = await FindAsync(id);
        if (await db.Orders.AnyAsync(o => o.Items.Any(i => i.ProductId == id)))
            throw new BusinessRuleException($"Không thể xóa '{product.Name}' vì đã có trong đơn hàng.");

        db.Products.Remove(product);
        await db.SaveChangesAsync();
    }

    /// <summary>Nhập kho (delta &gt; 0) hoặc xuất kho (delta &lt; 0); không cho phép tồn kho âm.</summary>
    public async Task<Product> AdjustStockAsync(int id, int delta)
    {
        var product = await FindAsync(id);
        if (product.StockQuantity + delta < 0)
            throw new BusinessRuleException(
                $"Không đủ tồn kho cho '{product.Name}' (còn {product.StockQuantity}, yêu cầu xuất {-delta}).");
        product.StockQuantity += delta;
        await db.SaveChangesAsync();
        return product;
    }

    private async Task<Product> FindAsync(int id) =>
        await db.Products.FirstOrDefaultAsync(p => p.Id == id)
        ?? throw new NotFoundException($"Không tìm thấy sản phẩm {id}.");

    private async Task EnsureSkuAvailableAsync(string sku, int? excludeId)
    {
        var normalized = sku.ToUpper();
        if (await db.Products.AnyAsync(p => p.Sku.ToUpper() == normalized && p.Id != excludeId))
            throw new BusinessRuleException($"SKU '{sku}' đã tồn tại.");
    }

    private async Task EnsureCategoryExistsAsync(int categoryId)
    {
        if (!await db.Categories.AnyAsync(c => c.Id == categoryId))
            throw new BusinessRuleException($"Danh mục {categoryId} không tồn tại.");
    }
}
