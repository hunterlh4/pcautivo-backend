using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Domain.Models;

public class Product
{
    public int Id { get; set; }
    public string? Sku { get; set; }
    public string? Name { get; set; }
    public int BrandId { get; set; }
    public int? ProductGroupId { get; set; }
    public MeasurementUnit? PresentationUnit { get; set; }
    public decimal? PresentationSize { get; set; }
    public MeasurementUnit? Unit { get; set; }
    public decimal? Size { get; set; }
    public ProductType ProductType { get; set; }
    public string? Description { get; set; }
    public decimal? MinimumStock { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public Brand? Brand { get; set; }
}

public class ProductCategory
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}