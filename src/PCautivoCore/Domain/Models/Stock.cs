namespace PCautivoCore.Domain.Models;

public class Stock
{
    public int Id { get; set; }
    public int StoreId { get; set; }
    public int ProductId { get; set; }
    public decimal Quantity { get; set; } // Total en almacén
    public decimal AssignedQuantity { get; set; } // Asignado a PropertyItems
    public decimal AvailableQuantity => Quantity - AssignedQuantity; // Disponible para asignar
    public decimal AveragePrice { get; set; }
    public decimal TotalValue { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    
    // Navigation properties
    public Store? Store { get; set; }
    public Product? Product { get; set; }
}