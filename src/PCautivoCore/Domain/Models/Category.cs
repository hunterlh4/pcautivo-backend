namespace PCautivoCore.Domain.Models;

public class Category
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int? ParentId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public class CategoryWithProduct
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int ProductId { get; set; }
}