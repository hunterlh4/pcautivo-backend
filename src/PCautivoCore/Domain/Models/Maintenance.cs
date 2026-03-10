using PCautivoCore.Domain.Enums;

namespace PCautivoCore.Domain.Models;

public class Maintenance
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string? PropertyName { get; set; } // Snapshot del nombre de la propiedad
    public int? RoomId { get; set; }
    public string? RoomName { get; set; } // Snapshot del nombre del ambiente
    public int UserCleaningId { get; set; } // Usuario asignado para limpiar
    public MaintenanceTypeEnum MaintenanceType { get; set; }
    public MaintenanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Property? Property { get; set; }
    public Room? Room { get; set; }
    public User? UserCleaning { get; set; }
}

public class MaintenanceItem
{
    public int Id { get; set; }
    public int MaintenanceId { get; set; }
    public int? RoomId { get; set; }
    public int ProductId { get; set; }
    public string? Sku { get; set; }
    public string? Name { get; set; }
    public int? ProductGroupId { get; set; }
    
    // Snapshot histórico - cómo se compra
    public MeasurementUnit? PresentationUnit { get; set; }
    public decimal? PresentationSize { get; set; }
    
    // Snapshot histórico - unidad base
    public MeasurementUnit? Unit { get; set; }
    public decimal? Size { get; set; }
    
    public bool IsVariable { get; set; } // Indica si los valores son fijos o pueden cambiar
  //  public decimal? QuantityUsed { get; set; } // Cantidad consumida en unidad base
    public bool IsConsumed { get; set; } // Indica si el producto fue consumido (0=No, 1=Sí)
    public ProductType ProductType { get; set; }
    public string? Comments { get; set; }
    public int CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public Room? Room { get; set; }
    public Product? Product { get; set; }
}
