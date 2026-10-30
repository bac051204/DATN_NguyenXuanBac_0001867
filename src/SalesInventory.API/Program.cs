using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using SalesInventory.Api.Data;
using SalesInventory.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình bí mật cục bộ (chuỗi kết nối...) không đưa vào Git.
// Tạo từ appsettings.Local.example.json, ví dụ: appsettings.Development.Local.json.
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<BusinessExceptionHandler>();

builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Cho phép WebApplicationFactory<Program> trong project test truy cập.
public partial class Program;
