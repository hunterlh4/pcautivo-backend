using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Domain.Models;

public enum OperationType
{
    Purchase = 0,      // Compra (siempre entrada)
    Out = 1,          // Salida de consumo (siempre salida)
    Transfer = 2,      // Transferencia (genera entrada Y salida)
    Consumption = 3,   // Consumo interno (siempre salida)
    Adjustment = 4,    // Ajuste de inventario (puede ser entrada o salida)
    Return = 5         // Devolución (puede ser entrada o salida)
}

public enum TransactionType
{
    In = 0,   // Entrada al almacén
    Out = 1   // Salida del almacén
}

public enum InventoryStatus
{
    Available = 0,
    Assigned = 1,
    Consumed = 2,
    Damaged = 3,
    Reserved = 4,
    Transferred = 5,
}

public class Inventory
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int StoreId { get; set; }  // Almacén donde está el item actualmente
    public string? Sku { get; set; }
    public string? SerialNumber { get; set; }  // Formato: SKU-00001
    public int? Correlativo { get; set; }  // Número correlativo usado para generar SerialNumber
    
    // Estado actual del item
    public InventoryStatus Status { get; set; }
    
    public string? OrderNumber { get; set; } // Número de orden/factura
    
    // Información de precio (snapshot al momento de la compra)
    public decimal? PriceBase { get; set; } // Precio por unidad base (por ml, por g, etc.)
    public decimal? PricePresentation { get; set; }  // Precio por unidad de presentación (ej: $10 por botella)
    
    // Información de unidades (snapshot al momento de la compra)
    public MeasurementUnit? UnitBase { get; set; }  // Unidad base (Milliliter, Gram, etc.)
    public MeasurementUnit? UnitPresentation { get; set; }  // Unidad de presentación (FluidOunce, etc.)
    
    // Tamaños originales
    public decimal? OriginalPresentationSize { get; set; }  // Tamaño original de presentación (ej: 32 para 32oz)
    public decimal? OriginalBaseSize { get; set; }  // Tamaño original en unidad base (ej: 946 para 32oz = 946ml)
    
    // Consumo acumulado
    public decimal ConsumedBase { get; set; }  // Ej: 300 ml consumidos
    public decimal ConsumedPresentation { get; set; }  // Ej: 10.6 oz consumidos
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    // Navigation properties
    public Product? Product { get; set; }
    public Store? Store { get; set; }
}
