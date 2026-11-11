using Microsoft.EntityFrameworkCore;
using SalesInventory.Infrastructure.Persistence;
using SalesInventory.Api.Dtos;
using SalesInventory.Domain.Entities;

namespace SalesInventory.Api.Services;

public class OrderService(AppDbContext db, TimeProvider clock)
{
    public async Task<List<Order>> GetAllAsync() =>
        await db.Orders.AsNoTracking().Include(o => o.Items).OrderBy(o => o.Id).ToListAsync();

    public async Task<Order> GetByIdAsync(int id) =>
        await db.Orders.AsNoTracking().Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id)
        ?? throw new NotFoundException($"Không tìm thấy đơn hàng {id}.");

    /// <summary>Tạo đơn bán hàng và trừ tồn kho. Cả đơn thất bại nếu một dòng không đủ hàng.</summary>
    public async Task<Order> CreateAsync(CreateOrderRequest request)
    {
        // Gộp các dòng trùng sản phẩm để kiểm tra tồn kho chính xác.
        var lines = request.Items
            .GroupBy(i => i.ProductId)
            .Select(g => (ProductId: g.Key, Quantity: g.Sum(i => i.Quantity)))
            .ToList();

        var productIds = lines.Select(l => l.ProductId).ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);

        var order = new Order
        {
            CustomerName = request.CustomerName,
            CreatedAt = clock.GetUtcNow().UtcDateTime,
            Status = OrderStatus.Completed
        };
        foreach (var line in lines)
        {
            if (!products.TryGetValue(line.ProductId, out var product))
                throw new NotFoundException($"Không tìm thấy sản phẩm {line.ProductId}.");
            if (product.StockQuantity < line.Quantity)
                throw new BusinessRuleException(
                    $"Không đủ tồn kho cho '{product.Name}' (còn {product.StockQuantity}, cần {line.Quantity}).");

            product.StockQuantity -= line.Quantity;
            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.SalePrice,
                Quantity = line.Quantity
            });
        }

        // Một lần SaveChanges = một transaction: trừ kho và tạo đơn cùng thành công hoặc cùng thất bại.
        db.Orders.Add(order);
        await db.SaveChangesAsync();
        return order;
    }

    /// <summary>Hủy đơn và hoàn trả số lượng về kho.</summary>
    public async Task<Order> CancelAsync(int id)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new NotFoundException($"Không tìm thấy đơn hàng {id}.");
        if (order.Status == OrderStatus.Cancelled)
            throw new BusinessRuleException($"Đơn hàng {id} đã bị hủy trước đó.");

        var productIds = order.Items.Select(i => i.ProductId).ToList();
        var products = await db.Products.Where(p => productIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id);
        foreach (var item in order.Items)
            products[item.ProductId].StockQuantity += item.Quantity;

        order.Status = OrderStatus.Cancelled;
        await db.SaveChangesAsync();
        return order;
    }
}
