namespace PCautivoCore.Domain.Models;

public class Property
{
    public int Id { get; set; }
    public int? ExternalId { get; set; }  // ID de Hostaway API (NULL si es creada manualmente)
    public int TypeId { get; set; }
    public string? Name { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Street { get; set; }
    // public int? PersonCapacity { get; set; }
    // public int? BedroomsNumber { get; set; }
    // public int? BedsNumber { get; set; }
    // public int? BathroomsNumber { get; set; }
    // public int? GuestBathroomsNumber { get; set; }
    public decimal? AverageReviewRating { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public PropertyType? Type { get; set; }
}

public class PropertyImage
{
    public int Id { get; set; }
    public int? ExternalId { get; set; }  // ID de Hostaway API
    public int PropertyId { get; set; }
    public string? ItemCaption { get; set; }
    public string? ItemUrl { get; set; }
    public int SortOrder { get; set; }
    public int Type { get; set; }  // 0=Api, 1=Local
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}