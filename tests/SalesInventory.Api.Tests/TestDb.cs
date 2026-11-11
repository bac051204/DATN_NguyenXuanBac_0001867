using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SalesInventory.Infrastructure.Persistence;

namespace SalesInventory.Api.Tests;

/// <summary>
/// SQLite in-memory: DB tồn tại khi connection còn mở, mỗi instance là một DB độc lập.
/// Schema tạo bằng EnsureCreated (gồm cả dữ liệu seed Category).
/// </summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public TestDb()
    {
        _connection.Open();
        using var db = CreateContext();
        db.Database.EnsureCreated();
    }

    public SqliteConnection Connection => _connection;

    public DbContextOptions<AppDbContext> Options =>
        new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

    /// <summary>Mỗi lần gọi tạo một context mới, giống một request riêng.</summary>
    public AppDbContext CreateContext() => new(Options);

    public void Dispose() => _connection.Dispose();
}
