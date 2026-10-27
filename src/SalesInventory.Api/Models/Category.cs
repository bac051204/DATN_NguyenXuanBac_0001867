using System.Text.Json.Serialization;

namespace SalesInventory.Api.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    [JsonIgnore]
    public List<Product> Products { get; set; } = new();
}
